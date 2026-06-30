using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WMS_Helper_Application.Model;

namespace WMS_Helper_Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window

        //instantiates barmmodels collection
    {
        public ObservableCollection<BarcodeModel> barcodeModels { get; } = new();
        public MainWindow()
        {
            Seed();
            InitializeComponent();
            DataContext = this;
        }

        public void Seed()
        {
            var seed = new[]
            {
                new BarcodeModel("teststring",11997, 2002021),
                new BarcodeModel("test2",11252, 2002210),
                new BarcodeModel("test3",11252, 235125)
            };

            foreach (var item in seed) {
               barcodeModels.Add(item);
            }

        }




        

        private void AddItemBtn_Click(object sender, RoutedEventArgs e)
        {
            AddItemWindow itemWindow = new AddItemWindow();

            itemWindow.Owner = this;
            itemWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            bool? returnedValue = itemWindow.ShowDialog();

        }

        private void addItemBtn_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}