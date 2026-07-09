using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using WMS_Helper_Application.Model;

namespace WMS_Helper_Application
{
    public partial class MainWindow : Window
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

        private void AvailableBarcodeLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AvailableBarcodeLB.SelectedItem is BarcodeModel item)
            {
                var app = (App)Application.Current;
                app.AvailableBarcodeModels.Remove(item);
                app.SelectedBarcodeModels.Add(item);
            }
        }

        private void SelectedBarcodeLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedBarcodeLB.SelectedItem is BarcodeModel item)
            {
                var app = (App)Application.Current;
                app.SelectedBarcodeModels.Remove(item);
                app.AvailableBarcodeModels.Add(item);
            }
        }

        private void printBtn_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;

            if (app.SelectedBarcodeModels.Count == 0)
            {
                MessageBox.Show("No items selected for printing.", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var print = new System.Windows.Controls.PrintDialog();
            if (print.ShowDialog() == true)
            {
                var pageSize  = new Size(print.PrintableAreaWidth, print.PrintableAreaHeight);
                var paginator = new BarcodeDocumentPaginator(app.SelectedBarcodeModels, pageSize);

                print.PrintDocument(paginator, "Barcode Report");
            }
        }
    }
}
