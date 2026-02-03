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

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Column(TypeName = "nvarchar(100)")]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; } = null!;

        [Column(TypeName = "varchar(10)")]
        public string? SoDienThoai { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        public string? DiaChi { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? AnhDaiDien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
        [Column(TypeName = "varchar(50)")]
        public string TaiKhoan { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Column(TypeName = "varchar(50)")]
        public string MatKhau { get; set; } = null!;

        public bool DangNhapGoogle { get; set; } = false;

        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public virtual ICollection<TuongTacDanhGia> TuongTacDanhGias { get; set; } = new List<TuongTacDanhGia>();
    }
}