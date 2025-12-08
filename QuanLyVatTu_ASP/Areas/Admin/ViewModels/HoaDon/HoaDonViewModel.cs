using QuanLyVatTu_ASP.Areas.Admin.Models;
using System.Collections.Generic;

namespace QuanLyVatTu_ASP.Areas.Admin.ViewModels
{
    public class HoaDonViewModel
    {
        public List<OrderForItem> Orders { get; set; } = new List<OrderForItem>();

        public class OrderForItem
        {
            public int DonHangId { get; set; }
            public string MaDonHang { get; set; }
            public string TenKhachHang { get; set; }
            public DateTime NgayDat { get; set; }
            public decimal TongTien { get; set; }
            public decimal SoTienDatCoc { get; set; }

            // Logic kiểm tra
            public decimal TyLeCoc => TongTien > 0 ? (SoTienDatCoc / TongTien) * 100 : 0;
            public bool DuDieuKienXuat => TyLeCoc >= 10;
            public int? HoaDonId { get; set; }
        }
    }

    // ViewModel cho trang Chi tiết hóa đơn
    public class HoaDonDetailViewModel
    {
        public int HoaDonId { get; set; }
        public string MaHoaDon { get; set; }
        public DateTime NgayLap { get; set; }

        // Thông tin công ty 
        public string TenCongTy => "CÔNG TY TNHH VẬT LIỆU XÂY DỰNG VUÝP";
        public string DiaChiCongTy => "123 Lê Văn Việt, Thủ Đức, TP.HCM";
        public string MSTCongTy => "0373456789";

        // Thông tin khách hàng
        public string TenKhachHang { get; set; }
        public string DiaChiKhachHang { get; set; }
        public string MSTKhachHang { get; set; }
        public string PhuongThucThanhToan { get; set; }

        // Danh sách vật tư
        public List<ChiTietItem> ChiTiet { get; set; } = new List<ChiTietItem>();

        // Tổng kết
        public decimal TongTienHang { get; set; }
        public decimal ThueGTGT { get; set; }
        public decimal TongThanhToan { get; set; }

        public class ChiTietItem
        {
            public int STT { get; set; }
            public string TenVatTu { get; set; }
            public string DVT { get; set; }
            public int SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
        }
    }
}
