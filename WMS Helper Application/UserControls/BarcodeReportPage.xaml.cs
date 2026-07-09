using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace WMS_Helper_Application
{
    /// <summary>
    /// A single printable page of the barcode report. The set of rows shown on
    /// this page is supplied through <see cref="Items"/>; pagination itself is
    /// handled by <see cref="BarcodeDocumentPaginator"/>.
    /// </summary>
    public partial class BarcodeReportPage : UserControl
    {
        public BarcodeReportPage()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(IEnumerable),
                typeof(BarcodeReportPage), new PropertyMetadata(null));

        public IEnumerable Items
        {
            get => (IEnumerable)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty PageInfoProperty =
            DependencyProperty.Register(nameof(PageInfo), typeof(string),
                typeof(BarcodeReportPage), new PropertyMetadata(string.Empty));

        public string PageInfo
        {
            get => (string)GetValue(PageInfoProperty);
            set => SetValue(PageInfoProperty, value);
        }
    }
}
