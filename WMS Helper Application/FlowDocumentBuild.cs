using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using WMS_Helper_Application.Model;

namespace WMS_Helper_Application
{
    internal static class FlowDocumentBuild
    {
        public static FlowDocument BuildReport(IEnumerable<BarcodeModel> items)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(96),
                ColumnWidth = double.MaxValue
            };

            doc.Blocks.Add(BuildTitle());
            doc.Blocks.Add(BuildItemTable(items));

            return doc;
        }

        private static Block BuildTitle()
        {
            return new Paragraph(new Run("Barcode Report"))
            {
                FontSize      = 20,
                FontWeight    = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin        = new Thickness(0, 0, 0, 24)
            };
        }

        private static Block BuildItemTable(IEnumerable<BarcodeModel> items)
        {
            var table = new Table { CellSpacing = 0 };

            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());

            var headerGroup = new TableRowGroup();
            var headerRow   = new TableRow { Background = Brushes.LightGray };

            headerRow.Cells.Add(HeaderCell("Item Name"));
            headerRow.Cells.Add(HeaderCell("Item Number"));
            headerRow.Cells.Add(HeaderCell("PO Number"));
            headerRow.Cells.Add(HeaderCell("Barcode"));

            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            var dataGroup = new TableRowGroup();
            bool alternate = false;

            foreach (var item in items)
            {
                var row = new TableRow
                {
                    Background = alternate ? Brushes.WhiteSmoke : Brushes.White
                };

                row.Cells.Add(DataCell(item.ItemName));
                row.Cells.Add(DataCell(item.ItemNumber.ToString()));
                row.Cells.Add(DataCell(item.ItemPurchaseOrderNumber.ToString()));
                row.Cells.Add(DataCell(item.ItemNumberToBarcode));

                dataGroup.Rows.Add(row);
                alternate = !alternate;
            }

            table.RowGroups.Add(dataGroup);
            return table;
        }

        private static TableCell HeaderCell(string text)
        {
            return new TableCell(new Paragraph(new Run(text))
            {
                FontWeight = FontWeights.Bold,
                Padding    = new Thickness(6, 4, 6, 4)
            })
            {
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(1)
            };
        }

        private static TableCell DataCell(string text)
        {
            return new TableCell(new Paragraph(new Run(text))
            {
                Padding = new Thickness(6, 4, 6, 4)
            })
            {
                BorderBrush     = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 1, 1)
            };
        }
    }
}
