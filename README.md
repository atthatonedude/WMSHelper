# WMSHelper

## Shared libraries

The reusable parts of WMS Helper live in two class libraries, so other projects can use them.

| Project | Target | What it holds |
|---|---|---|
| `WMSHelper.Core` | `net10.0`, any OS | `BarcodeModel` and `SqlLiteConnector` (SQLite storage through Dapper) |
| `WMSHelper.Wpf` | `net10.0-windows`, WPF | `WindowControl` title bar, barcode report printing (`BarcodePrinter`), the Barlow and barcode fonts, and the shared styles in `Themes/Styles.xaml` |

`WMSHelper.Wpf` references `WMSHelper.Core`. The tests in `tests/WMSHelper.Core.Tests` cover Core and run on any OS with `dotnet test`.

### Using them in a new WPF app

1. Reference both projects.
2. Merge the shared styles in `App.xaml`. This also brings in the Fluent theme and the `AppFont` / `BarcodeFont` resources:
   ```xml
   <Application.Resources>
       <ResourceDictionary>
           <ResourceDictionary.MergedDictionaries>
               <ResourceDictionary Source="pack://application:,,,/WMSHelper.Wpf;component/Themes/Styles.xaml"/>
           </ResourceDictionary.MergedDictionaries>
       </ResourceDictionary>
   </Application.Resources>
   ```
3. For the custom title bar, give the window a `WindowChrome` and add the control. Without `WindowChrome` you get the normal Windows title bar as well.
   ```xml
   <Window ...
           xmlns:controls="clr-namespace:WMSHelper.Wpf.Controls;assembly=WMSHelper.Wpf">
       <WindowChrome.WindowChrome>
           <WindowChrome GlassFrameThickness="0"/>
       </WindowChrome.WindowChrome>
       ...
       <controls:WindowControl Title="My App" Background="#1d2d3d"/>
   ```
   On dialogs, set `ShowMinMaxButtons="False"` to show only the close button.
4. For storage, the app decides where its database lives and passes the path in:
   ```csharp
   var connector = new SqlLiteConnector(databasePath);
   connector.Initialize();   // creates the folder, file and table if they're missing
   ```
5. For printing, `BarcodePrinter.Print(items)` shows the print dialog and prints the barcode report. It returns `false` if the user cancels.
