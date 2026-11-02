using System.Security.Policy;

namespace WMSHelper.ShellView.Model

{
    public class PrintItemModel
    {

        public PrintItemModel(string name)
        {
            Name = name;
            
        }
        public string Name { get; set; }
        public bool IsSelected { get; set; }

        public string Url { get; set; } // Add a property for the URL
    }
}