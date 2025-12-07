using QuanLyVatTu_ASP.Areas.Admin.Models;
using System.Collections.Generic;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class HoaDonViewModel
    {
        public HoaDon HoaDon { get; set; }
        public List<KhachHang> KhachHangs { get; set; }
        public List<QuanLyVatTu_ASP.Areas.Admin.Models.NhanVien> NhanViens { get; set; }

        public HoaDonViewModel()
        {
            HoaDon = new HoaDon();
            KhachHangs = new List<KhachHang>();
            NhanViens = new List<QuanLyVatTu_ASP.Areas.Admin.Models.NhanVien>();
        }
    }
}
