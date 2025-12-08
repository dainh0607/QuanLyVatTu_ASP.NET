// File: Models/ViewModels/ProfileViewModels.cs
using QuanLyVatTu_ASP.Areas.Admin.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu_ASP.Models.ViewModels
{
    public class ProfileViewModel
    {
        public KhachHang KhachHang { get; set; }
        public IEnumerable<DonHang> DonHangs { get; set; }
        public decimal TongTienMua { get; set; }
        public int SoDonHang { get; set; }
        public int SoSanPhamDaMua { get; set; }
    }

    public class ProfileUpdateModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public string SoDienThoai { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ không quá 255 ký tự")]
        public string DiaChi { get; set; }
    }

    public class ChangePasswordModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
        public string MatKhauCu { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string MatKhauMoi { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu không khớp")]
        public string XacNhanMatKhauMoi { get; set; }
    }
}