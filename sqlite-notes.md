# SQLite in WMS Helper Application — Notes

Working notes on adding SQLite persistence to the WPF app, comparing Dapper and
sqlite-net, plus where the connection string belongs.

---

## Starting point

`App.xaml.cs` had a broken stub:

```csharp
using var connection = new SqliteConnection();  // no connection string
connection.Open();
connection.create                                // dangling, won't compile
```

The csproj already references `Dapper` 2.1.79 and `Microsoft.Data.Sqlite` 10.0.10,
so Option A below needs no new packages.

---

## Where the .db file lives

Don't put it next to the .exe — if this ever installs to `Program Files`, that
directory is read-only for standard users. Use per-user app data:

```csharp
public static class DbPaths
{
    public static string DbFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WMSHelper", "wmshelper.db");

    public static string ConnectionString { get; } = new SqliteConnectionStringBuilder
    {
        DataSource = DbFilePath,
        Mode = SqliteOpenMode.ReadWriteCreate,
        Cache = SqliteCacheMode.Shared
    }.ToString();

    public static void EnsureFolder() =>
        Directory.CreateDirectory(Path.GetDirectoryName(DbFilePath)!);
}
```

SQLite creates the file on first connect; you only need the folder to exist.

---

## Breaking down the path property

```csharp
public static string DbFilePath { get; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "WMSHelper", "wmshelper.db");
```

Evaluates to `C:\Users\<you>\AppData\Local\WMSHelper\wmshelper.db`. It's just a
string — nothing is created or touched on disk by this line.

### `public static string DbFilePath { get; } = ...`

A **get-only auto-property with an initializer**. The compiler generates a
`private static readonly` backing field plus a getter:

```csharp
private static readonly string <DbFilePath>k__BackingField = Path.Combine(...);
public static string DbFilePath => <DbFilePath>k__BackingField;
```

- **Runs once.** The initializer executes in the static constructor, triggered
  the first time anything touches `DbPaths`.
- **Thread-safe for free.** The CLR guarantees type initializers run exactly
  once, even under concurrent access.
- **Immutable.** No setter, backing field is `readonly`.

### `Environment.GetFolderPath(SpecialFolder.LocalApplicationData)`

Windows doesn't guarantee user folders live under `C:\Users\<name>` — they can be
redirected by group policy, sit on another drive, or differ on a non-English
install. Ask the OS rather than hardcoding.

| SpecialFolder | Typical path | Roams to other machines? |
|---|---|---|
| `LocalApplicationData` | `C:\Users\<you>\AppData\Local` | No — stays on this PC |
| `ApplicationData` | `C:\Users\<you>\AppData\Roaming` | Yes, with roaming profiles |
| `CommonApplicationData` | `C:\ProgramData` | No — shared by all users on the PC |

`Local` is right for a SQLite file. Roaming profiles sync on logoff/logon, so a
multi-megabyte binary DB there means slow logins and corruption risk if the same
DB is open on two machines. `ProgramData` would suit shared-across-users data,
but brings write-permission and concurrency problems.

The key win over `Program Files`: this folder is user-writable by default. No
elevation, no UAC prompt, no installer ACL work.

### `Path.Combine(a, b, c)`

Joins segments with the platform separator, handling boundaries so you don't get
`AppData\Local\\WMSHelper` or a missing slash.

Gotcha: if a later segment is **rooted** (`"C:\temp"` or `"\foo"`),
`Path.Combine` discards everything before it. Safe with literals, a real footgun
once a segment comes from user input or config.

### What it does *not* do

- **Doesn't create the directory.** Hence `EnsureFolder()`.
  `Directory.CreateDirectory` is idempotent. SQLite creates the `.db` file
  itself but will **not** create a missing parent folder — you'd get a
  `SqliteException` on `Open()`.
- **Doesn't validate anything.** No writability or drive check.

### Naming note

Avoid naming it `File` — it collides with `System.IO.File`, which is in scope via
`ImplicitUsings` (enabled in the csproj). The property wins name resolution over
the type, making `File.Exists(...)` fail to compile inside that class. Hence
`DbFilePath`.

The `!` on `Path.GetDirectoryName(DbFilePath)!` is the null-forgiving operator —
the method returns `string?` (null for a root path like `"C:\"`), and `Nullable`
is enabled in the csproj. It can't be null here since the path always has a parent.

---

## Model changes needed first

`Model/BarcodeModel.cs` has two traits that break ORM round-tripping:

1. **No parameterless constructor.** Dapper can work around this (it matches the
   single ctor's parameter names to column names); sqlite-net cannot — it
   hard-requires one.
2. **`ItemNumberToBarcode` is derived, and its setter mutates.** It wraps the
   value in `*...*` and throws if the input already contains `*`. Persist that
   column and read it back and you get `**11997**` on the second load — or an
   `ArgumentException`. Treat it as computed, never store it.

```csharp
public class BarcodeModel : INotifyPropertyChanged
{
    public BarcodeModel() { }   // for ORM materialization

    public BarcodeModel(string name, int itemNumber, int itemPurchaseOrder) { /* existing */ }

    public int Id { get; set; }
    // ItemName, ItemNumber, ItemPurchaseOrderNumber unchanged
}
```

---

## Schema

This is **SQL**, not C# — SQLite's dialect:

```sql
CREATE TABLE IF NOT EXISTS Barcodes (
    Id                      INTEGER PRIMARY KEY AUTOINCREMENT,
    ItemName                TEXT    NOT NULL,
    ItemNumber              INTEGER NOT NULL,
    ItemPurchaseOrderNumber INTEGER NOT NULL
);
```

`INTEGER PRIMARY KEY AUTOINCREMENT` is picky — `INT` or a composite key won't
alias to `rowid`.

Notes:

- **`IF NOT EXISTS` makes it safe to run every startup.** First launch creates
  the table; every launch after is a no-op.
- **It only creates. It never migrates.** Add a `DateCreated` column to the model
  later and this statement won't touch an existing table. You'd need
  `ALTER TABLE Barcodes ADD COLUMN DateCreated TEXT;`. Proper handling is a
  schema-versioning concern — SQLite's `PRAGMA user_version` is the usual hook.
- **The types are SQLite storage classes, not C# types.** Microsoft.Data.Sqlite
  maps `INTEGER`/`TEXT` to `int`/`long` and `string` when Dapper materializes rows.

In Option B you never write this SQL — `CreateTable<T>()` generates it from
attributes. That's the core trade between the libraries: Dapper means you own the
SQL, sqlite-net means the library writes it.

---

## Option A — Dapper + Microsoft.Data.Sqlite (recommended)

Both packages already referenced.

```csharp
using Dapper;
using Microsoft.Data.Sqlite;

public class BarcodeRepository
{
    private SqliteConnection Open()
    {
        var cn = new SqliteConnection(DbPaths.ConnectionString);
        cn.Open();
        return cn;
    }

    public void Initialize()
    {
        DbPaths.EnsureFolder();
        using var cn = Open();
        cn.Execute("""
            CREATE TABLE IF NOT EXISTS Barcodes (
                Id                      INTEGER PRIMARY KEY AUTOINCREMENT,
                ItemName                TEXT    NOT NULL,
                ItemNumber              INTEGER NOT NULL,
                ItemPurchaseOrderNumber INTEGER NOT NULL
            );
            """);
    }

    public IEnumerable<BarcodeModel> GetAll()
    {
        using var cn = Open();
        // Explicit column list — never SELECT *, or a future column named
        // ItemNumberToBarcode would hit that mutating setter.
        return cn.Query<BarcodeModel>(
            "SELECT Id, ItemName, ItemNumber, ItemPurchaseOrderNumber FROM Barcodes ORDER BY ItemName;");
    }

    public int Insert(BarcodeModel item)
    {
        using var cn = Open();
        return cn.ExecuteScalar<int>("""
            INSERT INTO Barcodes (ItemName, ItemNumber, ItemPurchaseOrderNumber)
            VALUES (@ItemName, @ItemNumber, @ItemPurchaseOrderNumber);
            SELECT last_insert_rowid();
            """, item);
    }

    public void InsertMany(IEnumerable<BarcodeModel> items)
    {
        using var cn = Open();
        using var tx = cn.BeginTransaction();
        cn.Execute("""
            INSERT INTO Barcodes (ItemName, ItemNumber, ItemPurchaseOrderNumber)
            VALUES (@ItemName, @ItemNumber, @ItemPurchaseOrderNumber);
            """, items, tx);   // Dapper loops the list, reusing one prepared command
        tx.Commit();
    }

    public IEnumerable<BarcodeModel> SearchByPo(int po)
    {
        using var cn = Open();
        return cn.Query<BarcodeModel>(
            "SELECT Id, ItemName, ItemNumber, ItemPurchaseOrderNumber FROM Barcodes WHERE ItemPurchaseOrderNumber = @po;",
            new { po });
    }

    public void Delete(int id)
    {
        using var cn = Open();
        cn.Execute("DELETE FROM Barcodes WHERE Id = @id;", new { id });
    }
}
```

Wrapping bulk inserts in a transaction matters a lot with SQLite — without it
every `INSERT` is its own fsync, and a few hundred rows crawls.

**Package note:** `Dapper.Extensions.NetCore` in the csproj isn't Dapper's
official contrib library. It's a third-party wrapper for caching and
config-driven connections. If you wanted `Insert<T>`/`GetAll<T>` helpers, the
package is `Dapper.Contrib`. Otherwise drop the reference — plain Dapper covers
the above.

---

## Option B — sqlite-net

Different package, different native provider:

```xml
<PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
<PackageReference Include="SQLitePCLRaw.bundle_e_sqlite3" Version="2.1.10" />
```

Attributes go on the model directly:

```csharp
using SQLite;

[Table("Barcodes")]
public class BarcodeModel : INotifyPropertyChanged
{
    public BarcodeModel() { }

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(200), NotNull]
    public string ItemName { get; set; }

    [Indexed]
    public int ItemNumber { get; set; }

    public int ItemPurchaseOrderNumber { get; set; }

    [Ignore]                        // derived — must not round-trip
    public string ItemNumberToBarcode { get; set; }
}
```

```csharp
public class BarcodeRepository : IDisposable
{
    private readonly SQLiteConnection _db;

    public BarcodeRepository()
    {
        DbPaths.EnsureFolder();
        _db = new SQLiteConnection(DbPaths.DbFilePath);  // raw path, not a connection string
        _db.CreateTable<BarcodeModel>();                 // idempotent; also migrates added columns
    }

    public List<BarcodeModel> GetAll() =>
        _db.Table<BarcodeModel>().OrderBy(b => b.ItemName).ToList();

    public int Insert(BarcodeModel item) => _db.Insert(item);   // sets item.Id in place

    public void InsertMany(IEnumerable<BarcodeModel> items) =>
        _db.InsertAll(items, runInTransaction: true);

    public List<BarcodeModel> SearchByPo(int po) =>
        _db.Table<BarcodeModel>().Where(b => b.ItemPurchaseOrderNumber == po).ToList();

    public int Update(BarcodeModel item) => _db.Update(item);
    public int Delete(int id) => _db.Delete<BarcodeModel>(id);

    public void Dispose() => _db.Dispose();
}
```

**Trade-off:** sqlite-net gives you table creation, lightweight LINQ, and no SQL —
nice for a small local store. But it couples the domain model to persistence
attributes, its LINQ subset is thin (no joins, no grouping — drop to
`_db.Query<T>("...")` for anything real), and `CreateTable<T>` only adds columns;
it never drops or alters them.

**Don't install both.** Microsoft.Data.Sqlite and SQLitePCLRaw each bundle their
own native SQLite binary. Two copies in one process is a load-order headache for
no benefit.

---

## Where the connection string goes — not App.xaml

`App.xaml`'s `<Application.Resources>` is a XAML `ResourceDictionary` for UI
resources: styles, brushes, fonts, templates. You *can* technically put a
`<sys:String x:Key="ConnStr">` there, but shouldn't — it compiles into BAML
inside the assembly, so changing it means a rebuild, and it mixes configuration
into a purely presentational file.

**1. Build it in code (recommended here).** The path depends on
`LocalApplicationData`, which varies per machine and user — there's no fixed
string to configure. The `SqliteConnectionStringBuilder` above *is* the right
home. No config file needed.

**2. `App.config`** — if you want it user-editable post-deploy. Note that
`System.Configuration` on .NET 10 requires the
`System.Configuration.ConfigurationManager` NuGet package; it's no longer in the
base framework.

```xml
<configuration>
  <connectionStrings>
    <add name="WmsDb" connectionString="Data Source=|DataDirectory|wmshelper.db" />
  </connectionStrings>
</configuration>
```

```csharp
var cs = ConfigurationManager.ConnectionStrings["WmsDb"].ConnectionString;
```

**3. `appsettings.json` + `Microsoft.Extensions.Configuration`** — the modern
approach, worth it if you later add DI (`Microsoft.Extensions.Hosting`) and want
`IOptions<T>` and environment overrides. Overkill for one local file path.

---

## Wiring into startup

Replacing the hardcoded `Seed()` in `App.xaml.cs`:

```csharp
public partial class App : Application
{
    public BarcodeRepository Repository { get; } = new();
    public ObservableCollection<BarcodeModel> AvailableBarcodeModels { get; } = new();
    public ObservableCollection<BarcodeModel> SelectedBarcodeModels { get; } = new();

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        Repository.Initialize();

        if (!Repository.GetAll().Any())
            Repository.InsertMany(new[]
            {
                new BarcodeModel("teststring", 11997, 2002021),
                new BarcodeModel("test2",      11252, 2002210),
            });

        foreach (var item in Repository.GetAll())
            AvailableBarcodeModels.Add(item);
    }
}
```

Two WPF cautions once you move past startup: run queries off the UI thread
(`await Task.Run(() => Repository.GetAll())`) so the window doesn't freeze, but
mutate `ObservableCollection` only on the dispatcher thread — WPF throws if a
collection-changed notification arrives from a background thread.

---

## Whose method is `cn.Execute` / `cn.ExecuteScalar`?

Both are **Dapper's**, defined as extension methods in Dapper's static
`SqlMapper` class:

```csharp
namespace Dapper
{
    public static partial class SqlMapper
    {
        public static int Execute(
            this IDbConnection cnn, string sql, object? param = null,
            IDbTransaction? transaction = null, int? commandTimeout = null,
            CommandType? commandType = null) { ... }

        public static T? ExecuteScalar<T>(
            this IDbConnection cnn, string sql, object? param = null,
            IDbTransaction? transaction = null, int? commandTimeout = null,
            CommandType? commandType = null) { ... }

        // non-generic sibling, returns object?
        public static object? ExecuteScalar(this IDbConnection cnn, string sql, ...);
    }
}
```

The `this IDbConnection cnn` is what makes them extension methods — you can write
`cn.Execute(sql)` even though `SqliteConnection` has no such member. The compiler
rewrites it to `SqlMapper.Execute(cn, sql)`.

It works on `SqliteConnection` via the chain `SqliteConnection` → `DbConnection`
→ implements `IDbConnection`. Dapper never references SQLite at all — it extends
the ADO.NET interface, which is why the same Dapper code works against SQL
Server, Postgres, or SQLite by swapping only the connection type.

### The `using` gotcha

Extension methods are only visible when their namespace is imported. Drop
`using Dapper;` and `cn.Execute(...)` fails with *"'SqliteConnection' does not
contain a definition for 'Execute'"* — which reads like a missing package but is
a missing `using`. That error on a method you know exists is the usual tell.

### `ExecuteScalar` exists in two places

| Call | Whose | Returns |
|---|---|---|
| `cn.ExecuteScalar<int>(sql)` | Dapper extension on `IDbConnection` | `int` |
| `cmd.ExecuteScalar()` | ADO.NET, real method on `DbCommand` | `object?` |

`IDbConnection` has no `ExecuteScalar` of its own — connections don't execute
anything, commands do. So on a *connection* it's always Dapper; on a *command*
it's built-in ADO.NET. Dapper deliberately mirrored the name.

### What they do under the hood

`Execute` creates a `DbCommand`, sets `CommandText`, maps your parameter object's
properties to `SqliteParameter`s, calls `ExecuteNonQuery()`, and returns rows
affected:

```csharp
// Dapper
cn.Execute("DELETE FROM Barcodes WHERE Id = @id;", new { id });

// raw ADO.NET — same thing
using var cmd = cn.CreateCommand();
cmd.CommandText = "DELETE FROM Barcodes WHERE Id = @id;";
cmd.Parameters.AddWithValue("@id", id);
cmd.ExecuteNonQuery();
```

That collapsing of command creation, parameter binding, and disposal into one
line is essentially all Dapper is.

The return value is rows affected — meaningful for `DELETE`/`UPDATE`/`INSERT`,
not for DDL. Nothing is "affected" by `CREATE TABLE`, which is why `Initialize()`
ignores it.

`ExecuteScalar` returns **the first column of the first row**, discarding
everything else. No result-set iteration, no object materialization. In `Insert`
above, two statements run in one round trip: the `INSERT` produces no rows, then
`SELECT last_insert_rowid()` produces exactly one — that's what's returned.

The generic form saves two annoyances over raw ADO.NET:

```csharp
// raw ADO.NET
object? result = cmd.ExecuteScalar();   // object, not int
int id = Convert.ToInt32(result);       // cast required

// Dapper
int id = cn.ExecuteScalar<int>("SELECT last_insert_rowid();");
```

First the cast. Second, and easier to get wrong: `ExecuteScalar` returns
**`DBNull.Value`** when the column is SQL `NULL`, and **`null`** when the query
returned no rows at all. Different values, neither casts cleanly to `int`. Dapper
collapses both to `default(T)` — `ExecuteScalar<int>` gives `0`,
`ExecuteScalar<int?>` gives `null`.

SQLite detail: `last_insert_rowid()` returns a 64-bit `INTEGER`, so
`ExecuteScalar<long>` is the literal match. `<int>` works because Dapper
converts, and is fine unless you expect billions of rows.

### The three Dapper methods used here

| Method | Returns | Used for |
|---|---|---|
| `Execute` | `int` (rows affected) | `INSERT`/`UPDATE`/`DELETE`/DDL — no result set |
| `Query<T>` | `IEnumerable<T>` | `SELECT` — one object per row |
| `ExecuteScalar<T>` | `T` | `SELECT` returning a single value |

All are extension methods on `IDbConnection` from `SqlMapper`. There are async
twins (`ExecuteAsync`, `QueryAsync<T>`) worth using once DB calls move off the UI
thread — Microsoft.Data.Sqlite's async methods run synchronously under the hood
since SQLite has no async I/O, but they still keep work off the dispatcher when
combined with `Task.Run`.

---

## Raw string literals (`"""`)

The SQL in these examples uses C# 11 raw string literals. Everything between the
`"""` markers is the SQL — C# just carries it. No escaping needed for quotes or
backslashes, and the closing `"""` sets the indentation baseline: whatever
indentation it sits at gets stripped from every line. That's why the SQL can be
indented to match surrounding code without those spaces ending up in the string.
