using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("KhachHang")]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(TaiKhoan), IsUnique = true)]
    public class KhachHang : BaseEntity
    {
        [Column(TypeName = "varchar(20)")]
        public string MaHienThi { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string HoTen { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; } = null!;

        [Column(TypeName = "varchar(10)")]
        public string? SoDienThoai { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        public string? DiaChi { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string TaiKhoan { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string MatKhau { get; set; } = null!;

        public bool DangNhapGoogle { get; set; } = false;
    }
}