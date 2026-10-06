using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using WMSHelper.Core.Data;
using WMSHelper.Core.Models;

namespace WMSHelper.Core.Tests
{
    public class SqlLiteConnectorTests
    {
        private static BarcodeModel Item(string name, int number = 100, int purchaseOrder = 900) =>
            new(name, number, purchaseOrder);

        // ---------- Initialize ----------

        [Fact]
        public void Initialize_creates_the_database_file()
        {
            using var db = new TempDatabase(initialize: false);

            Assert.False(File.Exists(db.Path));

            db.Connector.Initialize();

            Assert.True(File.Exists(db.Path));
        }

        [Fact]
        public void Initialize_creates_the_containing_folder_when_it_is_missing()
        {
            using var db = new TempDatabase(initialize: false);

            Assert.False(Directory.Exists(Path.GetDirectoryName(db.Path)));

            db.Connector.Initialize();

            Assert.True(Directory.Exists(Path.GetDirectoryName(db.Path)));
        }

        [Fact]
        public void Initialize_is_safe_to_run_again_and_keeps_existing_rows()
        {
            using var db = new TempDatabase();
            db.Connector.Insert(Item("kept"));

            db.Connector.Initialize();

            Assert.Single(db.Connector.GetAll());
        }

        /// <summary>
        /// Regression: Initialize was once guarded on the database file existing.
        /// SQLite creates the file the moment a connection opens, so a launch cut
        /// short before the schema was written left a file with no table - and the
        /// guard then skipped the create on every launch after, permanently.
        /// </summary>
        [Fact]
        public void Initialize_creates_the_table_when_the_file_exists_without_it()
        {
            using var db = new TempDatabase(initialize: false);

            Directory.CreateDirectory(db.Folder);
            File.WriteAllBytes(db.Path, Array.Empty<byte>());
            Assert.True(File.Exists(db.Path));

            db.Connector.Initialize();

            // Would throw "no such table: Barcodes" if the create had been skipped.
            Assert.Empty(db.Connector.GetAll());
        }

        // ---------- GetAll ----------

        [Fact]
        public void GetAll_returns_nothing_for_a_fresh_database()
        {
            using var db = new TempDatabase();

            Assert.Empty(db.Connector.GetAll());
        }

        [Fact]
        public void GetAll_orders_by_item_name()
        {
            using var db = new TempDatabase();
            db.Connector.Insert(Item("zeta"));
            db.Connector.Insert(Item("alpha"));
            db.Connector.Insert(Item("mike"));

            Assert.Equal(
                new[] { "alpha", "mike", "zeta" },
                db.Connector.GetAll().Select(b => b.ItemName));
        }

        /// <summary>
        /// Regression: SQLite compares case-sensitively by default, so the list
        /// used to come back as Banana, apple, cherry.
        /// </summary>
        [Fact]
        public void GetAll_orders_by_item_name_ignoring_case()
        {
            using var db = new TempDatabase();
            db.Connector.Insert(Item("cherry"));
            db.Connector.Insert(Item("Banana"));
            db.Connector.Insert(Item("apple"));

            Assert.Equal(
                new[] { "apple", "Banana", "cherry" },
                db.Connector.GetAll().Select(b => b.ItemName));
        }

        [Fact]
        public void GetAll_maps_every_column_onto_the_model()
        {
            using var db = new TempDatabase();
            db.Connector.Insert(new BarcodeModel("widget", 4242, 5150));

            var loaded = Assert.Single(db.Connector.GetAll());

            Assert.Equal("widget", loaded.ItemName);
            Assert.Equal(4242, loaded.ItemNumber);
            Assert.Equal(5150, loaded.ItemPurchaseOrderNumber);
            Assert.True(loaded.Id > 0);
        }

        /// <summary>
        /// Regression: the barcode text used to be assigned in the three-argument
        /// constructor only. Dapper builds models through the parameterless one,
        /// so every row loaded from disk came back with it null and printed blank.
        /// </summary>
        [Fact]
        public void GetAll_returns_models_whose_barcode_text_is_populated()
        {
            using var db = new TempDatabase();
            db.Connector.Insert(new BarcodeModel("widget", 4242, 5150));

            var loaded = Assert.Single(db.Connector.GetAll());

            Assert.Equal("*4242*", loaded.ItemNumberToBarcode);
        }

        // ---------- Insert ----------

        [Fact]
        public void Insert_assigns_the_generated_id_back_onto_the_model()
        {
            using var db = new TempDatabase();
            var item = Item("thing");

            Assert.Equal(0, item.Id);

            db.Connector.Insert(item);

            Assert.True(item.Id > 0);
        }

        [Fact]
        public void Insert_hands_out_a_distinct_id_per_row()
        {
            using var db = new TempDatabase();
            var first = Item("first");
            var second = Item("second");

            db.Connector.Insert(first);
            db.Connector.Insert(second);

            Assert.NotEqual(first.Id, second.Id);
        }

        [Fact]
        public void Insert_stores_a_row_that_can_be_read_back_by_its_id()
        {
            using var db = new TempDatabase();
            var item = new BarcodeModel("readback", 11, 22);

            db.Connector.Insert(item);

            var loaded = db.Connector.GetAll().Single(b => b.Id == item.Id);
            Assert.Equal("readback", loaded.ItemName);
            Assert.Equal(11, loaded.ItemNumber);
            Assert.Equal(22, loaded.ItemPurchaseOrderNumber);
        }

        // ---------- InsertMany ----------

        [Fact]
        public void InsertMany_writes_every_row()
        {
            using var db = new TempDatabase();

            db.Connector.InsertMany(new[] { Item("a"), Item("b"), Item("c") });

            Assert.Equal(3, db.Connector.GetAll().Count);
        }

        [Fact]
        public void InsertMany_with_nothing_to_insert_leaves_the_table_alone()
        {
            using var db = new TempDatabase();

            db.Connector.InsertMany(Array.Empty<BarcodeModel>());

            Assert.Empty(db.Connector.GetAll());
        }

        /// <summary>
        /// Regression: InsertMany used to leave every model at Id 0, so editing
        /// one threw and deleting one silently removed nothing.
        /// </summary>
        [Fact]
        public void InsertMany_assigns_a_distinct_generated_id_to_every_model()
        {
            using var db = new TempDatabase();
            var items = new[] { Item("a"), Item("b"), Item("c") };

            db.Connector.InsertMany(items);

            Assert.All(items, item => Assert.True(item.Id > 0));
            Assert.Equal(3, items.Select(item => item.Id).Distinct().Count());

            // Each id has to point at that model's own row, not just any row.
            var stored = db.Connector.GetAll().ToDictionary(b => b.Id, b => b.ItemName);
            Assert.All(items, item => Assert.Equal(item.ItemName, stored[item.Id]));
        }

        [Fact]
        public void InsertMany_that_fails_part_way_stores_nothing_and_hands_out_no_ids()
        {
            using var db = new TempDatabase();
            var good = Item("good");
            var bad = new BarcodeModel { ItemName = null! }; // breaks NOT NULL

            Assert.Throws<SqliteException>(() => db.Connector.InsertMany(new[] { good, bad }));

            Assert.Equal(0, good.Id);
            Assert.Empty(db.Connector.GetAll());
        }

        // ---------- Update ----------

        [Fact]
        public void Update_reports_success_and_persists_every_field()
        {
            using var db = new TempDatabase();
            var item = new BarcodeModel("before", 1, 2);
            db.Connector.Insert(item);

            item.ItemName = "after";
            item.ItemNumber = 99;
            item.ItemPurchaseOrderNumber = 88;

            Assert.True(db.Connector.Update(item));

            var loaded = Assert.Single(db.Connector.GetAll());
            Assert.Equal("after", loaded.ItemName);
            Assert.Equal(99, loaded.ItemNumber);
            Assert.Equal(88, loaded.ItemPurchaseOrderNumber);
        }

        [Fact]
        public void Update_reports_failure_when_the_row_is_not_there()
        {
            using var db = new TempDatabase();

            var missing = Item("ghost");
            missing.Id = 999999;

            Assert.False(db.Connector.Update(missing));
        }

        [Fact]
        public void Update_refuses_a_model_that_was_never_inserted()
        {
            using var db = new TempDatabase();

            // Id 0 means "no row yet". Updating would silently match nothing,
            // which reads as success to a caller that ignores the return value.
            Assert.Throws<ArgumentException>(() => db.Connector.Update(Item("never saved")));
        }

        [Fact]
        public void Update_touches_only_the_targeted_row()
        {
            using var db = new TempDatabase();
            var target = Item("target");
            var bystander = new BarcodeModel("bystander", 7, 7);
            db.Connector.Insert(target);
            db.Connector.Insert(bystander);

            target.ItemName = "changed";
            db.Connector.Update(target);

            var loaded = db.Connector.GetAll().Single(b => b.Id == bystander.Id);
            Assert.Equal("bystander", loaded.ItemName);
            Assert.Equal(7, loaded.ItemNumber);
        }

        // ---------- Delete ----------

        [Fact]
        public void Delete_reports_success_and_removes_the_row()
        {
            using var db = new TempDatabase();
            var item = Item("doomed");
            db.Connector.Insert(item);

            Assert.True(db.Connector.Delete(item.Id));
            Assert.Empty(db.Connector.GetAll());
        }

        [Fact]
        public void Delete_reports_failure_when_the_row_is_not_there()
        {
            using var db = new TempDatabase();

            Assert.False(db.Connector.Delete(999999));
        }

        [Fact]
        public void Delete_by_model_removes_the_same_row_as_delete_by_id()
        {
            using var db = new TempDatabase();
            var item = Item("doomed");
            db.Connector.Insert(item);

            Assert.True(db.Connector.Delete(item));
            Assert.Empty(db.Connector.GetAll());
        }

        [Fact]
        public void Delete_removes_exactly_one_row()
        {
            using var db = new TempDatabase();
            var doomed = Item("doomed");
            db.Connector.InsertMany(new[] { Item("keep1"), Item("keep2") });
            db.Connector.Insert(doomed);

            db.Connector.Delete(doomed);

            var remaining = db.Connector.GetAll();
            Assert.Equal(2, remaining.Count);
            Assert.DoesNotContain(remaining, b => b.Id == doomed.Id);
        }

        // ---------- persistence and isolation ----------

        [Fact]
        public void Rows_survive_a_new_connector_over_the_same_file()
        {
            using var db = new TempDatabase();
            db.Connector.Insert(new BarcodeModel("persisted", 5, 6));

            // Stands in for closing and reopening the app.
            var reopened = db.NewConnector();
            reopened.Initialize();

            var loaded = Assert.Single(reopened.GetAll());
            Assert.Equal("persisted", loaded.ItemName);
            Assert.Equal(5, loaded.ItemNumber);
        }

        [Fact]
        public void Two_connectors_on_different_files_do_not_see_each_other()
        {
            using var first = new TempDatabase();
            using var second = new TempDatabase();

            first.Connector.Insert(Item("only in first"));

            Assert.Single(first.Connector.GetAll());
            Assert.Empty(second.Connector.GetAll());
        }

        [Fact]
        public void DatabasePath_reports_the_file_the_connector_was_given()
        {
            using var db = new TempDatabase();

            Assert.Equal(db.Path, db.Connector.DatabasePath);
        }
    }
}
