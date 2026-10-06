using System.IO;
using System.Runtime.CompilerServices;

namespace WMS_Helper_Application
{
    /// <summary>
    /// Where this app keeps its files. This lives in the app rather than in
    /// WMSHelper.Core so each app using the library picks its own location.
    /// </summary>
    internal static class AppPaths
    {
        public static string DatabasePath { get; } = GetDatabasePath();

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
        // project folder without hardcoding a username. Keep this file in the
        // project's root folder, or the debug database moves with it.
        private static string ProjectDirectory([CallerFilePath] string thisFile = "") =>
            Path.GetDirectoryName(thisFile)!;
    }
}
