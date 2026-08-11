using System.IO;
using Microsoft.Data.Sqlite;
using WMS_Helper_Application;

namespace WMSHelper.Tests
{
    /// <summary>
    /// A throwaway SQLite file for one test. xunit builds a new instance of the
    /// test class per test method, so each test gets its own database and they
    /// can safely run in any order.
    /// </summary>
    public sealed class TempDatabase : IDisposable
    {
        /// <summary>Its own folder, so a test can assert on the folder not existing yet.</summary>
        public string Folder { get; }

        public string Path { get; }

        public SqlLiteConnector Connector { get; }

        public TempDatabase(bool initialize = true)
        {
            Folder = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "wmshelper-tests",
                Guid.NewGuid().ToString("N"));

            Path = System.IO.Path.Combine(Folder, "wmshelper.db");

            Connector = new SqlLiteConnector(Path);

            if (initialize)
            {
                Connector.Initialize();
            }
        }

        /// <summary>A second connector over the same file - stands in for a restart.</summary>
        public SqlLiteConnector NewConnector() => new(Path);

        public void Dispose()
        {
            // Microsoft.Data.Sqlite pools connections. Without clearing the pool
            // the handle is still open, the delete fails, and %TEMP% fills up
            // with stray databases.
            SqliteConnection.ClearAllPools();

            try
            {
                if (Directory.Exists(Folder))
                {
                    Directory.Delete(Folder, recursive: true);
                }
            }
            catch (IOException)
            {
                // A leaked handle shouldn't fail an otherwise passing test.
            }
        }
    }
}
