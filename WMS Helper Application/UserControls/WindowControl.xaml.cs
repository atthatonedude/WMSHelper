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

namespace WMS_Helper_Application.UserControls
{
    /// <summary>
    /// Interaction logic for WindowControl.xaml
    /// </summary>
    public partial class WindowControl : UserControl
    {
        public WindowControl()
        {
            InitializeComponent();
        }

        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
        
            var window = Window.GetWindow(this);
            if (window != null) window.WindowState = WindowState.Minimized;
        }

        private void maximizeBTn_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
    
}
