using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class HoaDon : BaseEntity
    {
        [ForeignKey("DonHang")]
        public int MaDonHang { get; set; }

        [ForeignKey("NhanVien")]
        public int MaNhanVien { get; set; }

        [ForeignKey("KhachHang")]
        public int MaKhachHang { get; set; }

        [Column("NgayLap")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

        public decimal TongTienTruocThue { get; set; }
        public decimal TyLeThueGTGT { get; set; } = 10;

        // Cấu hình Computed để EF biết cột này do DB (Trigger) xử lý
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? TienThueGTGT { get; set; } 

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? TongTienSauThue { get; set; }

        public decimal ChietKhau { get; set; } = 0;
        public decimal SoTienDatCoc { get; set; } = 0;
        public string? PhuongThucThanhToan { get; set; }
        public string TrangThai { get; set; } = "Đã thanh toán";

        // Navigation
        public DonHang DonHang { get; set; } = null!;
        public NhanVien NhanVien { get; set; } = null!;
        public KhachHang KhachHang { get; set; } = null!;

        public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    }
}
