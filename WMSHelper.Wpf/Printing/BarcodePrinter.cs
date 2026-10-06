using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WMSHelper.Core.Models;

namespace WMSHelper.Wpf.Printing
{
    /// <summary>
    /// One-call printing of the barcode report: shows the print dialog, then
    /// lays the items out with <see cref="BarcodeDocumentPaginator"/>.
    /// </summary>
    public static class BarcodePrinter
    {
        // Returns false when the user cancels the print dialog.
        public static bool Print(IEnumerable<BarcodeModel> items, string description = "Barcode Report")
        {
            var print = new PrintDialog();

            if (print.ShowDialog() != true)
            {
                return false;
            }

            var pageSize  = new Size(print.PrintableAreaWidth, print.PrintableAreaHeight);
            var paginator = new BarcodeDocumentPaginator(items, pageSize);

            print.PrintDocument(paginator, description);

            return true;
        }
    }
}
