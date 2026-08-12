using System.Collections.ObjectModel;
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Connector.Initialize();

            var stored = Connector.GetAll();

            // First run only. Without the guard the sample rows get re-inserted
            // on every launch and the list grows forever.
            if (stored.Count == 0)
            {
                Connector.InsertMany(BuildSeedData());
                stored = Connector.GetAll();   // re-read so every model carries its Id
            }

            foreach (var item in stored)
            {
                AvailableBarcodeModels.Add(item);
            }
        }

        private BarcodeModel[] BuildSeedData()
        {
            return new[]
            {
                new BarcodeModel("teststring",11997, 2002021),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("teststring",11997, 2002021),

            };
        }



    }

}
