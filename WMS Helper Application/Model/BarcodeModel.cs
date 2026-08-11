using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WMS_Helper_Application.Model
{
    public class BarcodeModel : INotifyPropertyChanged
    {
        private string itemName = "";
        private int itemNumber;
        private int itemPurchaseOrderNumber;

        // Dapper materialises rows through this one, then the property setters.
        public BarcodeModel() { }

        public BarcodeModel(string name, int itemNumber, int itemPurchaseOrder)
        {
            ItemName = name;
            ItemNumber = itemNumber;
            ItemPurchaseOrderNumber = itemPurchaseOrder;
        }

        public int Id { get; set; }

        public string ItemName
        {
            get => itemName;
            set { itemName = value; OnPropertyChanged(); }
        }

        public int ItemNumber
        {
            get => itemNumber;
            set
            {
                itemNumber = value;
                OnPropertyChanged();
                // The barcode text is derived from this, so it changed too.
                OnPropertyChanged(nameof(ItemNumberToBarcode));
            }
        }

        public int ItemPurchaseOrderNumber
        {
            get => itemPurchaseOrderNumber;
            set { itemPurchaseOrderNumber = value; OnPropertyChanged(); }
        }

        // Derived rather than stored, so a model loaded from the database can
        // never come back missing it. Get-only also keeps Dapper from mapping it.
        public string ItemNumberToBarcode => $"*{ItemNumber}*";

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
