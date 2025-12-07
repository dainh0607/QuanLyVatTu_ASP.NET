using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels.KhachHangViewModels
{
    public class KhachHangCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
        [Display(Name = "Tài khoản")]
        public string TaiKhoan { get; set; } = null!;

        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
        public string? MatKhau { get; set; }

        [Display(Name = "Mã khách hàng")]
        public string MaHienThi => Id > 0 ? $"KH{Id:000}" : "Tự động tạo";
    }
}