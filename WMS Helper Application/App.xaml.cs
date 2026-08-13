using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using WMS_Helper_Application.Model;

namespace WMS_Helper_Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {


        public ObservableCollection<BarcodeModel> AvailableBarcodeModels { get; } = new();
        public ObservableCollection<BarcodeModel> SelectedBarcodeModels { get; } = new();

        // Reachable from any window via ((App)Application.Current).Connector
        public SqlLiteConnector Connector { get; } = new();

        // A new install starts empty. Sample rows used to be inserted whenever the
        // table had no rows, which meant emptying the list brought them back.
        //
        // The main window is opened here rather than through StartupUri so it only
        // appears once the database is known to be readable - StartupUri would show
        // it after this returns, leaving a live window over a dead connector.
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                Connector.Initialize();

                foreach (var item in Connector.GetAll())
                {
                    AvailableBarcodeModels.Add(item);
                }
            }
            catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException)
            {
                // This fires before any window exists. Left unhandled the process
                // dies to WPF's default handler and the user sees nothing at all -
                // clicking the icon simply appears to do nothing.
                MessageBox.Show(
                    "WMS Helper could not open its item database, so it has to close.\n\n" +
                    $"Database file:\n{Connector.DatabasePath}\n\n" +
                    $"Error:\n{ex.Message}\n\n" +
                    "If the file is damaged, rename it and start the app again - a new " +
                    "empty database will be created. Keep the old file if you want to " +
                    "try recovering the items it held.",
                    "WMS Helper",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(1);
                return;
            }

            new MainWindow().Show();
        }



    }

}
