namespace QuanLyVatTu_ASP.Areas.Admin.Models
{
    public class ChiTietDonHang
    {
        public int ID { get; set; }
        public int MaDonHang { get; set; }
        public int MaVatTu { get; set; }
        public int SoLuong { get; set; }
        public decimal? SoTienDatCoc { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;

        public DonHang DonHang { get; set; } = null!;
        public VatTu VatTu { get; set; } = null!;
    }
}
