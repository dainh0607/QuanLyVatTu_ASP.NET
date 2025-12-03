using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class VatTuViewModel
    {
        public QuanLyVatTu_ASP.Areas.Admin.Models.VatTu VatTu { get; set; }
        public List<QuanLyVatTu_ASP.Areas.Admin.Models.LoaiVatTu> LoaiVatTus { get; set; }
        public List<QuanLyVatTu_ASP.Areas.Admin.Models.NhaCungCap> NhaCungCaps { get; set; }

        public VatTuViewModel()
        {
            VatTu = new QuanLyVatTu_ASP.Areas.Admin.Models.VatTu();
            LoaiVatTus = new List<QuanLyVatTu_ASP.Areas.Admin.Models.LoaiVatTu>();
            NhaCungCaps = new List<QuanLyVatTu_ASP.Areas.Admin.Models.NhaCungCap>();
        }
    }
}
