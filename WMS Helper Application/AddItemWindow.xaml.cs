using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WMS_Helper_Application.Model;

namespace WMS_Helper_Application
{
    /// <summary>
    /// Interaction logic for AddItemWindow.xaml.
    /// Doubles as the edit window - use the constructor that takes an item.
    /// </summary>
    public partial class AddItemWindow : Window
    {
        // Null in add mode. In edit mode this is the instance the lists are
        // showing, kept aside so we only touch it once the save succeeds.
        private readonly BarcodeModel? original;

        public BarcodeModel? barcodeModel { get; set; }

        public AddItemWindow()
        {
            InitializeComponent();

            barcodeModel = new BarcodeModel("", 0, 0);
            DataContext = barcodeModel;
        }

        public AddItemWindow(BarcodeModel itemToEdit)
        {
            InitializeComponent();

            original = itemToEdit;

            // Bind to a copy. The text boxes write through on every keystroke, so
            // binding the real item would rewrite the row on screen even if this
            // window is closed without saving.
            barcodeModel = new BarcodeModel(
                itemToEdit.ItemName,
                itemToEdit.ItemNumber,
                itemToEdit.ItemPurchaseOrderNumber)
            {
                Id = itemToEdit.Id
            };

            DataContext = barcodeModel;

            // Window.Title still drives the taskbar entry, but the custom chrome
            // hides the OS caption, so the bar needs telling separately.
            Title = "Edit Item";
            titleBar.Title = "Edit Item";
            addItemBtn.Content = "Save Changes";
        }

        private void addItemBtn_Click(object sender, RoutedEventArgs e)
        {
            if (barcodeModel is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(barcodeModel.ItemName))
            {
                MessageBox.Show("Item name is required.", Title,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                itemNameTB.Focus();
                return;
            }

            if (barcodeModel.ItemNumber <= 0)
            {
                MessageBox.Show("Item number must be greater than zero.", Title,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                itemNumberTB.Focus();
                return;
            }

            var app = (App)Application.Current;

            try
            {
                if (original is null)
                {
                    // Save first. If the write fails we don't want the item sitting
                    // in the list looking like it was stored.
                    app.Connector.Insert(barcodeModel);
                    app.AvailableBarcodeModels.Add(barcodeModel);
                }
                else
                {
                    if (!app.Connector.Update(barcodeModel))
                    {
                        MessageBox.Show(
                            "That item is no longer in the database, so there was nothing to update.",
                            Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Copy back onto the instance the lists hold. Its property
                    // setters raise PropertyChanged, so the rows redraw.
                    original.ItemName = barcodeModel.ItemName;
                    original.ItemNumber = barcodeModel.ItemNumber;
                    original.ItemPurchaseOrderNumber = barcodeModel.ItemPurchaseOrderNumber;
                }
            }
            catch (SqliteException ex)
            {
                MessageBox.Show($"Could not save the item.\n\n{ex.Message}", Title,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
        }
    }
}
