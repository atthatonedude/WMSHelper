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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WMS_Helper_Application
{
    /// <summary>
    /// Interaction logic for TopBarUserControl.xaml
    /// </summary>
    public partial class TopBarUserControl : UserControl
    {
        public TopBarUserControl()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
