using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class DonHangIndexViewModel
    {
        public List<ItemViewModel> Items { get; set; } = new List<ItemViewModel>();

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; } = 0;
        public int TotalPages { get; set; } = 1;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public class ItemViewModel
        {
            public int ID { get; set; }
            public string MaHienThi { get; set; } = string.Empty;
            public string TenKhachHang { get; set; } = string.Empty;
            public string TenNhanVien { get; set; } = string.Empty;
            public string? HinhAnhNhanVien { get; set; }
            public DateTime NgayDat { get; set; }
            public decimal TongTien { get; set; }
            public decimal? SoTienDatCoc { get; set; }
            public string? PhuongThucDatCoc { get; set; }
            public DateTime? NgayDatCoc { get; set; }
            public string TrangThai { get; set; } = "Chờ xác nhận";
            public string? GhiChu { get; set; }
            public DateTime NgayTao { get; set; }
        }
    }
}