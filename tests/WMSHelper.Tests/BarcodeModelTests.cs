using System.ComponentModel;
using WMS_Helper_Application.Model;

namespace WMSHelper.Tests
{
    public class BarcodeModelTests
    {
        private static List<string> RecordChanges(BarcodeModel model)
        {
            var raised = new List<string>();
            model.PropertyChanged += (_, e) => raised.Add(e.PropertyName ?? "");
            return raised;
        }

        [Fact]
        public void Constructor_sets_the_properties_it_is_given()
        {
            var model = new BarcodeModel("widget", 4242, 5150);

            Assert.Equal("widget", model.ItemName);
            Assert.Equal(4242, model.ItemNumber);
            Assert.Equal(5150, model.ItemPurchaseOrderNumber);
        }

        [Fact]
        public void A_model_built_by_the_parameterless_constructor_has_a_usable_name()
        {
            // Dapper goes through this constructor. A null name would blow up the
            // list binding before anything got a chance to set it.
            var model = new BarcodeModel();

            Assert.NotNull(model.ItemName);
            Assert.Equal(0, model.Id);
        }

        [Theory]
        [InlineData(0, "*0*")]
        [InlineData(7, "*7*")]
        [InlineData(11997, "*11997*")]
        public void Barcode_text_wraps_the_item_number_in_asterisks(int itemNumber, string expected)
        {
            var model = new BarcodeModel("widget", itemNumber, 1);

            Assert.Equal(expected, model.ItemNumberToBarcode);
        }

        [Fact]
        public void Barcode_text_follows_the_item_number_when_it_changes()
        {
            var model = new BarcodeModel("widget", 1, 1);

            model.ItemNumber = 500;

            Assert.Equal("*500*", model.ItemNumberToBarcode);
        }

        [Fact]
        public void Barcode_text_never_double_wraps()
        {
            // The old settable version re-wrapped whatever it was handed, so a
            // round trip through the property turned *5* into **5**.
            var model = new BarcodeModel("widget", 5, 1);

            model.ItemNumber = model.ItemNumber;

            Assert.Equal("*5*", model.ItemNumberToBarcode);
        }

        [Fact]
        public void Setting_the_name_notifies_the_binding()
        {
            var model = new BarcodeModel("before", 1, 1);
            var raised = RecordChanges(model);

            model.ItemName = "after";

            Assert.Contains(nameof(BarcodeModel.ItemName), raised);
        }

        [Fact]
        public void Setting_the_item_number_notifies_the_barcode_text_as_well()
        {
            // The barcode is derived, so WPF is never told to re-read it unless
            // the number's setter says so explicitly.
            var model = new BarcodeModel("widget", 1, 1);
            var raised = RecordChanges(model);

            model.ItemNumber = 2;

            Assert.Contains(nameof(BarcodeModel.ItemNumber), raised);
            Assert.Contains(nameof(BarcodeModel.ItemNumberToBarcode), raised);
        }

        [Fact]
        public void Setting_the_purchase_order_notifies_the_binding()
        {
            var model = new BarcodeModel("widget", 1, 1);
            var raised = RecordChanges(model);

            model.ItemPurchaseOrderNumber = 42;

            Assert.Contains(nameof(BarcodeModel.ItemPurchaseOrderNumber), raised);
        }

        [Fact]
        public void A_model_is_observable_by_wpf()
        {
            Assert.IsAssignableFrom<INotifyPropertyChanged>(new BarcodeModel());
        }
    }
}
