using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels.KhachHangViewModels
{
    public class KhachHangIndexViewModel
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
            public string Email { get; set; } = string.Empty;
            public string? SoDienThoai { get; set; }
            public string? DiaChi { get; set; }
            public DateTime NgayTao { get; set; }
        }
    }
}
