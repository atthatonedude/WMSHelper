using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WMS_Helper_Application.Model
{
    public class BarcodeModel
    {   
        //instances with validation
        private string itemNumberToBarcode;

        public BarcodeModel(string name, int itemNumber,string itemNumberToBarcode, int itemPurchaseOrder) 
        {
            Name = name;
            ItemNumber = itemNumber;
            
            ItemPurchaseOrderNumber = itemPurchaseOrder;
        }

        public string Name {  get; set; }

        public int ItemNumber { get;set;  }


        public int ItemPurchaseOrderNumber { get; set; }

        public string ItemNumberToBarcode
        {
            get { return itemNumberToBarcode; }

            set 
            {
                if (!value.Contains("*")) 
                {
                    throw new ArgumentException("String cannot contain the * character");
                }
                else {

                    var formattedItem = $"*{value}*";

                    itemNumberToBarcode = formattedItem;
                }
                    
            }
        
        }



        
    }
}
