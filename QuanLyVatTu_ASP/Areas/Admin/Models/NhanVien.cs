using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Index(nameof(TaiKhoan), IsUnique = true)]
    public class NhanVien : BaseEntity
    {
        [Column(TypeName = "varchar(20)")]
        public string MaHienThi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Column(TypeName = "nvarchar(100)")]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        [Column(TypeName = "date")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập CCCD")]
        [Column(TypeName = "varchar(12)")]
        public string CCCD { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Column(TypeName = "varchar(10)")]
        public string SoDienThoai { get; set; } = null!;

        [Column(TypeName = "varchar(100)")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
        [Column(TypeName = "varchar(50)")]
        public string TaiKhoan { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Column(TypeName = "varchar(50)")]
        public string MatKhau { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]  
        [Column(TypeName = "nvarchar(50)")]
        public string VaiTro { get; set; } = null!;
    }
}
