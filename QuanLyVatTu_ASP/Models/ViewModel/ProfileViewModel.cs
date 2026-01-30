using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Models.ViewModels
{
    public class ProfileViewModel
    {
        public KhachHang KhachHang { get; set; } = null!;
        public IEnumerable<DonHang> DonHangs { get; set; } = Enumerable.Empty<DonHang>();

        public int SoDonHang => DonHangs?.Count() ?? 0;
        public decimal TongTienMua => DonHangs?.Sum(x => x.TongTien) ?? 0;
        public int SoSanPhamDaMua { get; set; }
    }
}