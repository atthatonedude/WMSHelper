using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace WMS_Helper_Application
{
    public class SqlLiteConnector
    {

        public static string SqlLiteLocation { get; } = GetDatabasePath();

        public static string ConnectionString { get; } = new SqliteConnectionStringBuilder
        {
            DataSource = SqlLiteLocation,
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


        public static void CreateSqliteDirectory()
        {
            // GetDirectoryName strips the file name off, so we create the folder
            // that holds the db and let SQLite create the file itself.
            Directory.CreateDirectory(Path.GetDirectoryName(SqlLiteLocation)!);
        }


        // The caller owns the returned connection - no `using` here, or it would
        // be disposed the moment this method returns.
        private SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(ConnectionString);

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

    }
}
