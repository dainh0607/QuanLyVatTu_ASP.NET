using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class NhaCungCapViewModel
    {
        public QuanLyVatTu_ASP.Areas.Admin.Models.NhaCungCap NhaCungCap { get; set; }

        public NhaCungCapViewModel()
        {
            NhaCungCap = new QuanLyVatTu_ASP.Areas.Admin.Models.NhaCungCap();
        }
    }
}
