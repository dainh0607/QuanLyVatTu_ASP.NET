using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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

        [ForeignKey("KhachHang")]
        public int MaKhachHang { get; set; }

        [Column("NgayLap")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TongTienTruocThue { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TyLeThueGTGT { get; set; } = 10;

        // Tính trong Service C#, không dùng Computed DB
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TienThueGTGT { get; set; } 

        // Tính trong Service C#, không dùng Computed DB
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TongTienSauThue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ChietKhau { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoTienDatCoc { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string? PhuongThucThanhToan { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string TrangThai { get; set; } = "Đã thanh toán";

        // Navigation
        [ForeignKey("MaDonHang")]
        public DonHang DonHang { get; set; } = null!;

        [ForeignKey("MaNhanVien")]
        public NhanVien NhanVien { get; set; } = null!;

        [ForeignKey("MaKhachHang")]
        public KhachHang KhachHang { get; set; } = null!;

        public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>(); 
    }
}
