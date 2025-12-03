using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class DonHangViewModel
    {
        public DonHang DonHang { get; set; }
        public List<QuanLyVatTu_ASP.Areas.Admin.Models.KhachHang> KhachHangs { get; set; }
        public List<QuanLyVatTu_ASP.Areas.Admin.Models.NhanVien> NhanViens { get; set; }

        public DonHangViewModel()
        {
            DonHang = new DonHang();
            KhachHangs = new List<QuanLyVatTu_ASP.Areas.Admin.Models.KhachHang>();
            NhanViens = new List<QuanLyVatTu_ASP.Areas.Admin.Models.NhanVien>();
        }
    }
}
