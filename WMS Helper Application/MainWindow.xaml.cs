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
            InitializeComponent();
            DataContext = this;
        }

        public void Seed()
        {
            var seed = new[] 
            {
                new BarcodeModel("6500A",25001,)
            };

        }




        private void printBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void addItemBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}