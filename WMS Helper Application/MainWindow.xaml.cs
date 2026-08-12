using Microsoft.Data.Sqlite;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using WMS_Helper_Application.Model;

namespace WMS_Helper_Application
{
    public partial class MainWindow : Window
    {
        //availabel barcodes Icollection view just views the collection and does not changes actual ObservableCollection
        private readonly ICollectionView availableBarcodesCollection;

        public MainWindow()
        {
            InitializeComponent();
            var currentApplicationInstance = (App)Application.Current;

            ICollectionView collection = CollectionViewSource.GetDefaultView(currentApplicationInstance.AvailableBarcodeModels);

            availableBarcodesCollection = collection;
            collection.Filter = FilterByString;

            DataContext = Application.Current;
        }

        private bool FilterByString(object obj)
        {
            var text = inputTB.Text;

            // Empty box shows everything rather than nothing.
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            return obj is BarcodeModel model
                && model.ItemName.Contains(text, StringComparison.OrdinalIgnoreCase);
        }
        private void AddItemBtn_Click(object sender, RoutedEventArgs e)
        {
            AddItemWindow itemWindow = new AddItemWindow();

            itemWindow.Owner = this;
            itemWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            bool? returnedValue = itemWindow.ShowDialog();
        }

        private void editItemBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableBarcodeLB.SelectedItem is not BarcodeModel item)
            {
                MessageBox.Show("Pick an item in the left hand list to edit.", "Edit Item",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // The window saves and copies the changes back onto `item` itself.
            var editWindow = new AddItemWindow(item)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            editWindow.ShowDialog();
        }

        private void deleteItemBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableBarcodeLB.SelectedItem is not BarcodeModel item)
            {
                MessageBox.Show("Pick an item in the left hand list to delete.", "Delete Item",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Delete \"{item.ItemName}\" permanently?", "Delete Item",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            var app = (App)Application.Current;

            try
            {
                // A false return means the row was already gone. Nothing to undo,
                // so fall through and drop it from the list either way.
                app.Connector.Delete(item);
            }
            catch (SqliteException ex)
            {
                MessageBox.Show($"Could not delete the item.\n\n{ex.Message}", "Delete Item",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            app.AvailableBarcodeModels.Remove(item);
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

        

       

        private void placeholderTB_GotFocus(object sender, RoutedEventArgs e)
        {
            placeholderTB.Visibility = Visibility.Collapsed;
            inputTB.Focus();
        }

       

        private void inputTB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            availableBarcodesCollection.Refresh();
        }
    }
}
