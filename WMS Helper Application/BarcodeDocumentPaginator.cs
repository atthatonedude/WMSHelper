using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using WMS_Helper_Application.Model;

namespace WMS_Helper_Application
{
    /// <summary>
    /// Paginates the barcode report by laying out a XAML <see cref="BarcodeReportPage"/>
    /// and measuring how many rows fit within the printable area, splitting the
    /// remaining items onto further pages.
    /// </summary>
    internal sealed class BarcodeDocumentPaginator : DocumentPaginator
    {
        private readonly List<BarcodeModel> _items;
        private Size _pageSize;
        private List<List<BarcodeModel>> _pages;

        public BarcodeDocumentPaginator(IEnumerable<BarcodeModel> items, Size pageSize)
        {
            _items = items.ToList();
            _pageSize = pageSize;
            Paginate();
        }

        public override bool IsPageCountValid => true;

        public override int PageCount => _pages.Count;

        public override Size PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                Paginate();
            }
        }

        public override IDocumentPaginatorSource Source => null;

        public override DocumentPage GetPage(int pageNumber)
        {
            var page = CreatePage(_pages[pageNumber]);
            page.PageInfo = $"Page {pageNumber + 1} of {_pages.Count}";

            page.Measure(_pageSize);
            page.Arrange(new Rect(new Point(0, 0), _pageSize));
            page.UpdateLayout();

            return new DocumentPage(page, _pageSize, new Rect(_pageSize), new Rect(_pageSize));
        }

        private BarcodeReportPage CreatePage(IEnumerable<BarcodeModel> rows) =>
            new BarcodeReportPage { Items = rows.ToList() };

        /// <summary>
        /// Groups items into pages by adding rows one at a time and measuring until
        /// the page control would exceed the printable height.
        /// </summary>
        private void Paginate()
        {
            _pages = new List<List<BarcodeModel>>();
            var current = new List<BarcodeModel>();

            foreach (var item in _items)
            {
                current.Add(item);

                var probe = CreatePage(current);
                probe.Measure(new Size(_pageSize.Width, double.PositiveInfinity));

                // Overflowed: pull this row back onto a fresh page (but always keep
                // at least one row per page so a single oversized row still prints).
                if (probe.DesiredSize.Height > _pageSize.Height && current.Count > 1)
                {
                    current.RemoveAt(current.Count - 1);
                    _pages.Add(current);
                    current = new List<BarcodeModel> { item };
                }
            }

            if (current.Count > 0)
            {
                _pages.Add(current);
            }
        }
    }
}
