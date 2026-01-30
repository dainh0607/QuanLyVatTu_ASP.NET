using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class HoaDon : BaseEntity
    {
        [Required]
        [ForeignKey("DonHang")]
        public int MaDonHang { get; set; }

        [Required]
        [ForeignKey("NhanVien")]
        public int MaNhanVien { get; set; }

        [Required]
        [ForeignKey("KhachHang")]
        public int MaKhachHang { get; set; }

        [Required]
        [Column("NgayLap")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTienTruocThue { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TyLeThueGTGT { get; set; } = 10;

        // Cấu hình Computed để EF biết cột này do DB (Trigger) xử lý
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TienThueGTGT { get; set; } 

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TongTienSauThue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ChietKhau { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienDatCoc { get; set; } = 0;

        [Column(TypeName = "nvarchar(50)")]
        public string? PhuongThucThanhToan { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string TrangThai { get; set; } = "Đã thanh toán";

        // Navigation
        public DonHang DonHang { get; set; } = null!;
        public NhanVien NhanVien { get; set; } = null!;
        public KhachHang KhachHang { get; set; } = null!;

        public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>(); 
    }
}
