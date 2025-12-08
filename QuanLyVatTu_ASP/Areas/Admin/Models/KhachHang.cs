using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class KhachHang : BaseEntity
    [Table("KhachHang")]
    public class KhachHang
    {
        public string MaHienThi => $"KH{ID:000}";
        public string HoTen { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public string TaiKhoan { get; set; } = null!;
        public string MatKhau { get; set; } = null!;

        [NotMapped]
        public bool DangNhapGoogle { get; set; } = false;
        [NotMapped]
        public string? MaGheGoogle { get; set; }
    }
}