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
    /// Interaction logic for AddItemWindow.xaml
    /// </summary>
    public partial class AddItemWindow : Window
    {
        public BarcodeModel? barcodeModel { get; set; } = new("",0,0);

        public AddItemWindow()
        {
            InitializeComponent();
            DataContext = barcodeModel;
        }

        private void addItemBtn_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            app.AvailableBarcodeModels.Add(barcodeModel);
            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
