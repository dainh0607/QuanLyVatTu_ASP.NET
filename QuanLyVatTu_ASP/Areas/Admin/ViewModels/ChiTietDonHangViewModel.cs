using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class ChiTietDonHangViewModel
    {
        public ChiTietDonHang ChiTiet { get; set; }
        public List<QuanLyVatTu_ASP.Areas.Admin.Models.VatTu> VatTus { get; set; }

        public ChiTietDonHangViewModel()
        {
            ChiTiet = new ChiTietDonHang();
            VatTus = new List<QuanLyVatTu_ASP.Areas.Admin.Models.VatTu>();
        }
    }
}
