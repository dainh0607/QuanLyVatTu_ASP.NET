using QuanLyVatTu_ASP.Areas.Admin.Models;
using System.Collections.Generic;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class ChiTietHoaDonViewModel
    {
        public ChiTietHoaDon ChiTietHoaDon { get; set; }
        public List<HoaDon> HoaDons { get; set; }
        public List<QuanLyVatTu_ASP.Areas.Admin.Models.LoaiVatTu> LoaiVatTus { get; set; }
        public List<QuanLyVatTu_ASP.Areas.Admin.Models.NhaCungCap> NhaCungCaps { get; set; }

        public ChiTietHoaDonViewModel()
        {
            ChiTietHoaDon = new ChiTietHoaDon();
            HoaDons = new List<HoaDon>();
            LoaiVatTus = new List<QuanLyVatTu_ASP.Areas.Admin.Models.LoaiVatTu>();
            NhaCungCaps = new List<QuanLyVatTu_ASP.Areas.Admin.Models.NhaCungCap>();
        }
    }
}
