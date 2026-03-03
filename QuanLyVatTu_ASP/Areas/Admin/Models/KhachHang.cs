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

        [MaxLength(150, ErrorMessage = "Số nhà/Tên đường không được vượt quá 150 ký tự")]
        [Column(TypeName = "nvarchar(150)")]
        public string? SoNhaTenDuong { get; set; }

        [MaxLength(100, ErrorMessage = "Phường/Xã không được vượt quá 100 ký tự")]
        [Column(TypeName = "nvarchar(100)")]
        public string? PhuongXa { get; set; }

        [MaxLength(100, ErrorMessage = "Tỉnh/Thành phố không được vượt quá 100 ký tự")]
        [Column(TypeName = "nvarchar(100)")]
        public string? TinhThanhPho { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? AnhDaiDien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
        [Column(TypeName = "varchar(50)")]
        public string TaiKhoan { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Column(TypeName = "varchar(255)")]
        public string MatKhau { get; set; } = null!;

        public bool DangNhapGoogle { get; set; } = false;

        [Column("DiemTichLuy", TypeName = "int")]
        public int DiemTichLuy { get; set; } = 0;

        [Column("MaHangThanhVien")]
        public int? MaHangThanhVien { get; set; }

        [Column("NgayLenHang", TypeName = "datetime")]
        public DateTime? NgayLenHang { get; set; }

        [Column("NgayHetHanHang", TypeName = "datetime")]
        public DateTime? NgayHetHanHang { get; set; }

        public bool NhanThongBaoDonHang { get; set; } = true;
        public bool NhanThongBaoKhuyenMai { get; set; } = true;
        public bool NhanThongBaoHangThanhVien { get; set; } = true;
        public bool TrangThaiKhoa { get; set; } = false;

        [ForeignKey("MaHangThanhVien")]
        public virtual HangThanhVien? HangThanhVien { get; set; }

        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public virtual ICollection<TuongTacDanhGia> TuongTacDanhGias { get; set; } = new List<TuongTacDanhGia>();
        public virtual ICollection<YeuThich> YeuThichs { get; set; } = new List<YeuThich>();
        public virtual ICollection<DiaChiNhanHang> DiaChiNhanHangs { get; set; } = new List<DiaChiNhanHang>();
        public virtual ICollection<ViVoucherKhachHang> ViVoucherKhachHangs { get; set; } = new List<ViVoucherKhachHang>();
        public virtual ICollection<LichSuSuDungVoucher> LichSuSuDungVouchers { get; set; } = new List<LichSuSuDungVoucher>();
        public virtual ICollection<LichSuTichDiem> LichSuTichDiems { get; set; } = new List<LichSuTichDiem>();
    }
}