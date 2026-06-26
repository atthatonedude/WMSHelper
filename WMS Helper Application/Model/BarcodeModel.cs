using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WMS_Helper_Application.Model
{
    public class BarcodeModel
    {   

        public BarcodeModel(string name, int itemNumber,string itemNumbertoBarcode, int itemPurchaseOrder) 
        {
            Name = name;
            ItemNumber = itemNumber;
            ItemNumberToBarcodeFormat = itemNumbertoBarcode;
            ItemPurchaseOrderNumber = itemPurchaseOrder;
        }

        public string Name {  get; set; }

        public int ItemNumber { get;set;  }

        private string ItemNumberToBarcodeFormat { get; set; }

        public int ItemPurchaseOrderNumber { get; set; }

        
    }
}
