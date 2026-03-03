using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email phải có đuôi @gmail.com")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải có 10 số và bắt đầu bằng 0")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [MaxLength(150, ErrorMessage = "Số nhà/Tên đường không được vượt quá 150 ký tự")]
        [Display(Name = "Số nhà / Tên đường / Khu phố")]
        public string? SoNhaTenDuong { get; set; }

        [MaxLength(100, ErrorMessage = "Phường/Xã không được vượt quá 100 ký tự")]
        [Display(Name = "Phường / Xã / Thị trấn")]
        public string? PhuongXa { get; set; }

        [MaxLength(100, ErrorMessage = "Tỉnh/Thành phố không được vượt quá 100 ký tự")]
        [Display(Name = "Tỉnh / Thành phố")]
        public string? TinhThanhPho { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
        [Display(Name = "Tài khoản")]
        public string TaiKhoan { get; set; } = null!;

        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
        public string? MatKhau { get; set; }

        [Display(Name = "Mã khách hàng")]
        public string MaHienThi { get; set; } = "Tự động tạo";

        // Upload ảnh đại diện
        [Display(Name = "Ảnh đại diện")]
        public IFormFile? AnhDaiDienFile { get; set; }

        // Đường dẫn ảnh hiện tại (dùng khi Edit)
        public string? AnhDaiDien { get; set; }

        // Thông tin thành viên (read-only trên UI)
        [Display(Name = "Đăng nhập Google")]
        public bool DangNhapGoogle { get; set; }

        [Display(Name = "Điểm tích lũy")]
        public int DiemTichLuy { get; set; }

        [Display(Name = "Hạng thành viên")]
        public string? TenHangThanhVien { get; set; }

        [Display(Name = "Ngày lên hạng")]
        public DateTime? NgayLenHang { get; set; }

        [Display(Name = "Ngày hết hạn hạng")]
        public DateTime? NgayHetHanHang { get; set; }
    }
}