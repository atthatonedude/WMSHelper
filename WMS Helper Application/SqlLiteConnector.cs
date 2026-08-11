using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using WMS_Helper_Application.Model;

namespace WMS_Helper_Application
{
    public class SqlLiteConnector
    {

        public static string SqlLiteLocation { get; } = GetDatabasePath();

        public static string ConnectionString { get; } = BuildConnectionString(SqlLiteLocation);

        // Which file this particular connector talks to. The app uses the
        // default; tests hand in a scratch path so they never touch it.
        public string DatabasePath { get; }

        private readonly string connectionString;

        public SqlLiteConnector() : this(SqlLiteLocation) { }

        public SqlLiteConnector(string databasePath)
        {
            DatabasePath = databasePath;
            connectionString = BuildConnectionString(databasePath);
        }

        public static string BuildConnectionString(string databasePath) =>
            new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            }.ToString();


        private static string GetDatabasePath()
        {
#if DEBUG
            // Keep the db next to the source in debug builds so it's easy to open
            // in a db browser. ProjectDirectory() is baked in at compile time, so
            // this is only meaningful on the machine that built it.
            return Path.Combine(ProjectDirectory(), "wmshelper.db");
#else
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WMSHelper", "wmshelper.db");
#endif
        }

        // The compiler substitutes this source file's full path, so we get the
        // project folder without hardcoding a username.
        private static string ProjectDirectory([CallerFilePath] string thisFile = "") =>
            Path.GetDirectoryName(thisFile)!;


        private void CreateSqliteDirectory()
        {
            // GetDirectoryName strips the file name off, so we create the folder
            // that holds the db and let SQLite create the file itself. A bare
            // file name has no directory part, which is fine - nothing to make.
            var folder = Path.GetDirectoryName(DatabasePath);

            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }


        // The caller owns the returned connection - no `using` here, or it would
        // be disposed the moment this method returns.
        private SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(connectionString);

            connection.Open();

            return connection;
        }

        public void Initialize()
        {
            CreateSqliteDirectory();

            using var openedConnection = OpenConnection();
            openedConnection.Execute("""
            CREATE TABLE IF NOT EXISTS Barcodes (
                Id                      INTEGER PRIMARY KEY AUTOINCREMENT,
                ItemName                TEXT    NOT NULL,
                ItemNumber              INTEGER NOT NULL,
                ItemPurchaseOrderNumber INTEGER NOT NULL
            );
            """);
        }





        // Explicit column list rather than SELECT *, so adding a column to the
        // table later can't silently change what gets mapped onto the model.
        public List<BarcodeModel> GetAll()
        {
            using var connection = OpenConnection();

            return connection.Query<BarcodeModel>("""
                SELECT Id, ItemName, ItemNumber, ItemPurchaseOrderNumber
                FROM Barcodes
                ORDER BY ItemName;
                """).ToList();
        }

        public void Insert(BarcodeModel item)
        {
            using var connection = OpenConnection();

            // last_insert_rowid() runs on the same connection, so it returns the
            // id from the INSERT above. Assigning it back means the caller's
            // object can be updated or deleted afterwards.
            item.Id = connection.ExecuteScalar<int>("""
                INSERT INTO Barcodes (ItemName, ItemNumber, ItemPurchaseOrderNumber)
                VALUES (@ItemName, @ItemNumber, @ItemPurchaseOrderNumber);
                SELECT last_insert_rowid();
                """, item);
        }

        // One transaction for the whole batch. Without it SQLite syncs to disk
        // once per row, which is painfully slow for a seed load.
        public void InsertMany(IEnumerable<BarcodeModel> items)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();

            connection.Execute("""
                INSERT INTO Barcodes (ItemName, ItemNumber, ItemPurchaseOrderNumber)
                VALUES (@ItemName, @ItemNumber, @ItemPurchaseOrderNumber);
                """, items, transaction);

            transaction.Commit();
        }

        // Returns false when no row matched, so a caller working from a stale
        // list can tell the difference between "saved" and "that row is gone".
        public bool Update(BarcodeModel item)
        {
            if (item.Id == 0)
            {
                throw new ArgumentException(
                    "Cannot update an item that has never been inserted (Id is 0).", nameof(item));
            }

            using var connection = OpenConnection();

            return connection.Execute("""
                UPDATE Barcodes
                SET ItemName                = @ItemName,
                    ItemNumber              = @ItemNumber,
                    ItemPurchaseOrderNumber = @ItemPurchaseOrderNumber
                WHERE Id = @Id;
                """, item) == 1;
        }

        public bool Delete(BarcodeModel item) => Delete(item.Id);

        public bool Delete(int id)
        {
            using var connection = OpenConnection();

            // Anonymous object rather than the model: DELETE only needs the key.
            return connection.Execute("DELETE FROM Barcodes WHERE Id = @id;", new { id }) == 1;
        }


    }


}
