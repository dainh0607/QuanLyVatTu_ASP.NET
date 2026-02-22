using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class HoaDonVAT : BaseEntity
    {
        // Auto-generated invoice number in DB
        // [DatabaseGenerated(DatabaseGeneratedOption.Computed)] <- Removed to allow manual insert attempts if needed
        [Column(TypeName = "nvarchar(50)")]
        public string? SoHoaDon { get; set; }

        [Required]
        public int MaDonHang { get; set; }

        [Required]
        public int MaKhachHang { get; set; }

        // Buyer info
        [Required]
        [Column(TypeName = "nvarchar(200)")]
        public string TenCongTy { get; set; } = null!;

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public string MaSoThue { get; set; } = null!;

        [Required]
        [Column(TypeName = "nvarchar(500)")]
        public string DiaChiDKKD { get; set; } = null!;

        [Column(TypeName = "nvarchar(200)")]
        public string? EmailNhanHoaDon { get; set; }

        // Seller info (pre-filled)
        [Column(TypeName = "nvarchar(200)")]
        public string TenNguoiBan { get; set; } = "CTY MÁY THIẾT BỊ KIM LONG";

        [Column(TypeName = "nvarchar(20)")]
        public string MaSoThueBan { get; set; } = "0123456789";

        [Column(TypeName = "nvarchar(500)")]
        public string DiaChiNguoiBan { get; set; } = "TP. Hồ Chí Minh";

        // Tax calculation
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTienTruocThue { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal ThueSuat { get; set; } = 10; // 10% VAT

        [Column(TypeName = "decimal(18,2)")]
        public decimal TienThue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTienSauThue { get; set; }

        // Metadata
        public DateTime NgayLap { get; set; } = DateTime.Now;

        [Column(TypeName = "nvarchar(50)")]
        public string TrangThai { get; set; } = "Đã xuất";

        [Column(TypeName = "nvarchar(500)")]
        public string? GhiChu { get; set; }

        // Navigation properties
        [ForeignKey("MaDonHang")]
        public virtual DonHang DonHang { get; set; } = null!;

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang KhachHang { get; set; } = null!;
    }
}
