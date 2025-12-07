namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels.VatTu
{
    public class VatTuIndexViewModel
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
            public string TenVatTu { get; set; } = string.Empty;
            public string DonViTinh { get; set; } = string.Empty;
            public decimal GiaNhap { get; set; }
            public decimal GiaBan { get; set; }
            public int SoLuongTon { get; set; }
            public string TenLoaiVatTu { get; set; } = string.Empty;
            public string TenNhaCungCap { get; set; } = string.Empty;
            public DateTime NgayTao { get; set; }
        }
    }
}
