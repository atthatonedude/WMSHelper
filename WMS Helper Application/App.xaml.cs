using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Seed();
            
        }

        private void Seed()
        {
            var seed = new[]
            {
                new BarcodeModel("teststring",11997, 2002021),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("teststring",11997, 2002021),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test2",11252, 2002210),
            };

            foreach (var item in seed)
            {
                AvailableBarcodeModels.Add(item);
            }

            using var connection = new SqliteConnection();
            connection.Open();

            connection.create
            
        }
    }

}
