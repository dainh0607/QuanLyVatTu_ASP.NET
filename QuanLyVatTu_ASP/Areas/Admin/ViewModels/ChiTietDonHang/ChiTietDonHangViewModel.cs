using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class ChiTietDonHangViewModel
    {
        public int MaDonHang { get; set; }

        public string? TenKhachHang { get; set; }
        public DateTime? NgayTao { get; set; }
        public string? SearchTerm { get; set; }
        public List<VatTuSelectItem> DanhSachVatTu { get; set; } = new();
        public List<ChiTietDonHangItem> ChiTietDonHang { get; set; } = new();
        public decimal TongTien => ChiTietDonHang.Sum(x => x.ThanhTien);
        
        // Extended for Invoice Logic
        public int? HoaDonId { get; set; }
        public decimal? SoTienDatCoc { get; set; }
        public bool DuDieuKienXuat => (SoTienDatCoc ?? 0) >= (TongTien * 0.1M) && TongTien > 0;
    }

    public class VatTuSelectItem
    {
        public int MaVatTu { get; set; }
        public string MaCode { get; set; } = string.Empty;
        public string TenVatTu { get; set; } = string.Empty;
        public string TenLoai { get; set; } = string.Empty;
        public int SoLuongTon { get; set; }
        public decimal GiaBan { get; set; }
    }

    public class ChiTietDonHangItem
    {
        public int MaVatTu { get; set; }
        public string TenVatTu { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;

        public bool IsSelected { get; set; }
    }
}
