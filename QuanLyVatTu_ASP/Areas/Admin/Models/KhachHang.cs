using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    [Table("KhachHang")]
    public class KhachHang
    {
        [Key]
        public int ID { get; set; }

        public string MaHienThi { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        public string Email { get; set; }

        [StringLength(15, ErrorMessage = "Số điện thoại không quá 15 ký tự")]
        public string SoDienThoai { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ không quá 255 ký tự")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Tài khoản là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tài khoản không quá 50 ký tự")]
        public string TaiKhoan { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(255, ErrorMessage = "Mật khẩu không quá 255 ký tự")]
        public string MatKhau { get; set; }

        public bool DangNhapGoogle { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}