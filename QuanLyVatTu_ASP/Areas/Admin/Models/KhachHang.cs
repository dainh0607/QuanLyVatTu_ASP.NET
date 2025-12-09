using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("KhachHang")]
    public class KhachHang : BaseEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string MaHienThi { get; set; }

        [Required]
        public string HoTen { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        public string? SoDienThoai { get; set; }

        public string? DiaChi { get; set; }

        public string TaiKhoan { get; set; } = null!;

        [Required]
        public string MatKhau { get; set; } = null!;

        public bool DangNhapGoogle { get; set; } = false;
    }
}