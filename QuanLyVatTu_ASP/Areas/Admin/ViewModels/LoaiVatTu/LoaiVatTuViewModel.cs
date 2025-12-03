using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class LoaiVatTuViewModel
    {
        public QuanLyVatTu_ASP.Areas.Admin.Models.LoaiVatTu LoaiVatTu { get; set; }

        public LoaiVatTuViewModel()
        {
            LoaiVatTu = new QuanLyVatTu_ASP.Areas.Admin.Models.LoaiVatTu();
        }
    }
}
