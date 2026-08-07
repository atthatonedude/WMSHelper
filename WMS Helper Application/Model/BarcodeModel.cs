using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace WMS_Helper_Application.Model
{
    public class BarcodeModel : INotifyPropertyChanged
    {   
        //instance with validation
        private string itemNumberToBarcode;
        
        public BarcodeModel() { }
        public BarcodeModel(string name, int itemNumber, int itemPurchaseOrder) 
        {
            ItemName = name;
            ItemNumber = itemNumber;
            ItemPurchaseOrderNumber = itemPurchaseOrder;
            ItemNumberToBarcode = itemNumber.ToString();
        }

        public string ItemName {  get; set; }

        public int ItemNumber { get;set;  }

        public int Id { get; set; }


        public int ItemPurchaseOrderNumber { get; set; }

        public string ItemNumberToBarcode
        {
            get { return itemNumberToBarcode; }

            set 
            {
                if (value.Contains("*")) 
                {
                    throw new ArgumentException("String cannot contain the * character");
                }
                else {

                    var formattedItem = $"*{value}*";

                    itemNumberToBarcode = formattedItem;
                }
                    
            }
        
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
