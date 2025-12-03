using System.Collections.Generic;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class NhanVienViewModel
    {
        public QuanLyVatTu_ASP.Areas.Admin.Models.NhanVien NhanVien { get; set; }
        public List<QuanLyVatTu_ASP.Areas.Admin.Models.NhanVien> NhanViens { get; set; }

        public NhanVienViewModel()
        {
            NhanVien = new QuanLyVatTu_ASP.Areas.Admin.Models.NhanVien();
            NhanViens = new List<QuanLyVatTu_ASP.Areas.Admin.Models.NhanVien>();
        }
    }
}
