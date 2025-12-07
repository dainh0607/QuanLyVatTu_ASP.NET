namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhanVien
{
    public class NhanVienIndexViewModel
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
            public string HoTen { get; set; } = string.Empty;
            public DateTime NgaySinh { get; set; }
            public string CCCD { get; set; } = string.Empty;
            public string SoDienThoai { get; set; } = string.Empty;
            public string? Email { get; set; }
            public string VaiTro { get; set; } = string.Empty;
            public DateTime NgayTao { get; set; }
        }
    }
}
