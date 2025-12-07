namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels.Admin.LoaiVatTu
{
    public class LoaiVatTuIndexViewModel
    {
        public List<ItemViewModel> Items { get; set; } = new();
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public class ItemViewModel
        {
            public int ID { get; set; }
            public string MaHienThi { get; set; } = string.Empty;
            public string TenLoaiVatTu { get; set; } = string.Empty;
            public string? MoTa { get; set; }
            public int SoLuongVatTu { get; set; }
            public DateTime NgayTao { get; set; }
        }
    }
}
