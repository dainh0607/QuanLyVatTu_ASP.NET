using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class DonHang : BaseEntity
    {
        public string MaHienThi => $"DH{DateTime.Now:yyyy}-{ID:000}";

        [Column("MaKhachHang")]
        public int KhachHangId { get; set; } // giữ tên property trong code, map tới MaKhachHang

        [Column("MaNhanVien")]
        public int NhanVienId { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;
        public decimal TongTien { get; set; } = 0;
        public decimal? SoTienDatCoc         { get; set; }
        public string? PhuongThucDatCoc { get; set; }
        public DateTime? NgayDatCoc { get; set; }
        public string TrangThai { get; set; } = "Chờ xác nhận";
        public string? GhiChu { get; set; }

        public KhachHang KhachHang { get; set; } = null!;
        public NhanVien? NhanVien { get; set; }

    }
}
