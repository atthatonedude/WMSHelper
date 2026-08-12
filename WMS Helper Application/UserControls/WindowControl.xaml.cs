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
        // Frozen so every instance can share the one default instance safely.
        private static readonly Geometry SquareIcon = FrozenGeometry("M 1,1 H 11 V 11 H 1 Z");

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title), typeof(string), typeof(WindowControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.Register(
                nameof(IconData), typeof(Geometry), typeof(WindowControl),
                new PropertyMetadata(SquareIcon));

        // Dialogs get the close button only. Defaults to true so the main
        // window keeps all three without having to opt in.
        public static readonly DependencyProperty ShowMinMaxButtonsProperty =
            DependencyProperty.Register(
                nameof(ShowMinMaxButtons), typeof(bool), typeof(WindowControl),
                new PropertyMetadata(true));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Geometry IconData
        {
            get => (Geometry)GetValue(IconDataProperty);
            set => SetValue(IconDataProperty, value);
        }

        public bool ShowMinMaxButtons
        {
            get => (bool)GetValue(ShowMinMaxButtonsProperty);
            set => SetValue(ShowMinMaxButtonsProperty, value);
        }

        public WindowControl()
        {
            InitializeComponent();
        }

        private static Geometry FrozenGeometry(string pathData)
        {
            var geometry = Geometry.Parse(pathData);
            geometry.Freeze();
            return geometry;
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
