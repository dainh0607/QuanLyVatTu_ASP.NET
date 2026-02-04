using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Models;

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
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email phải có đuôi @gmail.com")]
        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải có 10 số và bắt đầu bằng 0")]
        [Column(TypeName = "varchar(10)")]
        public string? SoDienThoai { get; set; }

        [MaxLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        [Column(TypeName = "nvarchar(255)")]
        public string? DiaChi { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? AnhDaiDien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
        [Column(TypeName = "varchar(50)")]
        public string TaiKhoan { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Column(TypeName = "varchar(255)")]
        public string MatKhau { get; set; } = null!;

        public bool DangNhapGoogle { get; set; } = false;

        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public virtual ICollection<TuongTacDanhGia> TuongTacDanhGias { get; set; } = new List<TuongTacDanhGia>();
        public virtual ICollection<YeuThich> YeuThichs { get; set; } = new List<YeuThich>();
        public virtual ICollection<DiaChiNhanHang> DiaChiNhanHangs { get; set; } = new List<DiaChiNhanHang>();
    }
}