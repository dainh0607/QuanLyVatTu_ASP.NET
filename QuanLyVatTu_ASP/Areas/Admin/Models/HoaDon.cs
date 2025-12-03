namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class HoaDon : BaseEntity
    {
        public int MaDonHang { get; set; }
        public int MaNhanVien { get; set; }
        public int MaKhachHang { get; set; }
        public DateTime NgayLap { get; set; } = DateTime.Now;

        // Tổng tiền hàng hóa trước thuế
        public decimal TongTienTruocThue { get; set; }

        // Tỷ lệ thuế GTGT (0 hoặc 10)
        public decimal TyLeThueGTGT { get; set; } = 10;

        // Tiền thuế GTGT = TongTienTruocThue * TyLeThueGTGT / 100
        public decimal? TienThueGTGT { get; private set; }

        // Tổng tiền sau thuế + thuế - chiết khấu
        public decimal? TongTienSauThue { get; private set; }

        public decimal ChietKhau { get; set; } = 0;
        public decimal SoTienDatCoc { get; set; } = 0;
        public string? PhuongThucThanhToan { get; set; }
        public string TrangThai { get; set; } = "Đã thanh toán";

        // Navigation
        public DonHang DonHang { get; set; } = null!;
        public NhanVien NhanVien { get; set; } = null!;
        public KhachHang KhachHang { get; set; } = null!;

        public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

        // Phương thức tự động tính thuế (gọi trước khi SaveChanges)
        public void TinhThueVaTongTien()
        {
            TienThueGTGT = Math.Round(TongTienTruocThue * TyLeThueGTGT / 100, 0);
            TongTienSauThue = Math.Round(TongTienTruocThue + TienThueGTGT.Value - ChietKhau, 0);
        }
    }
}
