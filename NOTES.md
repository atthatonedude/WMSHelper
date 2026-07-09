# WMSHelper - Project Notes

## What We Built
A WPF barcode report printing feature. Users select items from a list, click Print, and a formatted report is sent to the printer via a FlowDocument.

## Key Files
- `FlowDocumentBuild.cs` — static class that builds the FlowDocument report
- `MainWindow.xaml.cs` — print button handler (`printBtn_Click`)
- `Model/BarcodeModel.cs` — data model with barcode formatting logic
- `App.xaml.cs` — holds `AvailableBarcodeModels` and `SelectedBarcodeModels` as `ObservableCollection<BarcodeModel>`

## Printing Architecture
- `PrintDialog.ShowDialog()` gets printer selection and printable area dimensions
- `FlowDocumentBuild.BuildReport(items)` returns a `FlowDocument` populated with the selected items
- `PageWidth` and `PageHeight` are set from `print.PrintableAreaWidth/Height` after dialog confirms
- The `FlowDocument` is cast to `IDocumentPaginatorSource` to get its paginator
- `print.PrintDocument(paginator, "Barcode Report")` sends it to the printer

## FlowDocument Structure
- Title: bold centered `Paragraph`
- Body: `Table` with 4 columns — Item Name, Item Number, PO Number, Barcode
- Alternating row background (White / WhiteSmoke)
- `ColumnWidth = double.MaxValue` forces single-column layout
- `PagePadding = new Thickness(96)` gives 1-inch margins (96 DIPs = 1 inch in WPF)

## BarcodeModel Notes
- `ItemNumberToBarcode` wraps the item number in `*...*` for Code 39 barcode format
- Constructor must set through the property (`ItemNumberToBarcode = ...`) not the backing field directly, otherwise the `*` formatting is skipped

## Concepts Discussed
- `DocumentPaginator` — abstract class the print system drives page-by-page via `GetPage(int)`
- `FlowDocument` route vs custom `DocumentPaginator` — FlowDocument handles pagination automatically but sacrifices precise coordinate control; custom paginator uses `Canvas` with explicit coordinates for pixel-perfect layout
- `Block` / `Paragraph` / `Section` — FlowDocument content model; `Paragraph` holds inlines (`Run`, `Bold`, `LineBreak`), `Section` groups blocks with shared formatting
- `FormattedText` — used to measure text dimensions before placing in a precise layout
- WPF units: 96 DIPs = 1 inch; US Letter = 816 x 1056 units

## Warnings (Non-Breaking)
- `BarcodeModel.cs` CS8618: compiler warns backing field may be null — false positive, setter always assigns. Fix: initialize `private string itemNumberToBarcode = string.Empty;`
- `AddItemWindow.xaml.cs` CS8604: possible null reference on `Add()` call — pre-existing, not related to printing
