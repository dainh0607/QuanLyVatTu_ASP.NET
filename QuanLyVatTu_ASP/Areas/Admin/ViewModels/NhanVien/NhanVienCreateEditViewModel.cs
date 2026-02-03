using System.ComponentModel.DataAnnotations;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhanVien
{
    public class NhanVienCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; } = DateTime.Now.AddYears(-20);

        [Required(ErrorMessage = "Vui lòng nhập số CCCD")]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "CCCD phải đúng 12 số")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD chỉ được chứa 12 chữ số")]
        [Display(Name = "Số CCCD")]
        public string CCCD { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tài khoản đăng nhập")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Tài khoản phải từ 4-50 ký tự")]
        [Display(Name = "Tài khoản")]
        public string TaiKhoan { get; set; } = null!;

        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        public string? MatKhau { get; set; } // Chỉ bắt buộc khi tạo mới

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = null!;

        // Hiển thị mã nhân viên (chỉ đọc)
        [Display(Name = "Mã nhân viên")]
        public string MaHienThi { get; set; } = "Tự động tạo";
    }
}
