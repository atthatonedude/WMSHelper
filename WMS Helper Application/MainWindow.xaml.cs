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
        
        public MainWindow()
        {
            
            InitializeComponent();
            DataContext = Application.Current;
        }

        




        

        private void AddItemBtn_Click(object sender, RoutedEventArgs e)
        {
            AddItemWindow itemWindow = new AddItemWindow();

            itemWindow.Owner = this;
            itemWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            bool? returnedValue = itemWindow.ShowDialog();

        }

        
    }
}