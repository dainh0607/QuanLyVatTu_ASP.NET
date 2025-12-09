using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class HoaDonService : IHoaDonService
    {
        private readonly ApplicationDbContext _context;

        public HoaDonService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HoaDonViewModel> GetOrdersForIndexAsync()
        {
            var orders = await _context.DonHang
                .Include(d => d.KhachHang)
                .OrderByDescending(d => d.NgayTao)
                .Select(d => new HoaDonViewModel.OrderForItem
                {
                    DonHangId = d.ID,
                    MaDonHang = d.MaHienThi,
                    TenKhachHang = d.KhachHang.HoTen,
                    NgayDat = d.NgayDat,
                    TongTien = d.TongTien,
                    SoTienDatCoc = d.SoTienDatCoc ?? 0,
                    // Logic tìm xem đơn này đã có hóa đơn chưa
                    HoaDonId = _context.HoaDons
                                .Where(hd => hd.MaDonHang == d.ID)
                                .Select(hd => hd.ID)
                                .FirstOrDefault()
                })
                .ToListAsync();

            // Xử lý logic hiển thị ID (nếu = 0 thì gán null để View dễ check)
            foreach (var item in orders)
            {
                if (item.HoaDonId == 0) item.HoaDonId = null;
            }

            return new HoaDonViewModel { Orders = orders };
        }

        public async Task<(string Error, int NewInvoiceId)> CreateInvoiceFromOrderAsync(int donHangId)
        {
            // 1. Lấy thông tin đơn hàng + chi tiết
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.ID == donHangId);

            if (donHang == null) return ("Đơn hàng không tồn tại", 0);

            // 2. Kiểm tra đã xuất hóa đơn chưa
            if (await _context.HoaDons.AnyAsync(h => h.MaDonHang == donHangId))
            {
                return ("Đơn hàng này đã được xuất hóa đơn!", 0);
            }

            // 3. Kiểm tra tỷ lệ đặt cọc (Logic nghiệp vụ)
            decimal tyLe = donHang.TongTien > 0 ? ((donHang.SoTienDatCoc ?? 0) / donHang.TongTien) : 0;
            if (tyLe < 0.1m)
            {
                return ($"Chưa đủ điều kiện! Khách mới cọc {tyLe:P0}. Cần tối thiểu 10%.", 0);
            }

            // 4. Tạo Header Hóa đơn
            var hoaDon = new HoaDon
            {
                MaDonHang = donHang.ID,
                MaNhanVien = donHang.NhanVienId ?? 1, // Mặc định 1 nếu null (cần cẩn thận chỗ này tùy data thật)
                MaKhachHang = donHang.KhachHangId ?? 0,
                NgayLap = DateTime.Now,
                TongTienTruocThue = donHang.TongTien,
                TyLeThueGTGT = 10,
                ChietKhau = 0,
                SoTienDatCoc = donHang.SoTienDatCoc ?? 0,
                PhuongThucThanhToan = donHang.PhuongThucDatCoc,
                TrangThai = "Đã xuất"
            };

            // Lưu lần 1 để lấy ID Hóa đơn
            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync();

            // 5. Tạo Chi tiết hóa đơn (Copy từ Chi tiết đơn hàng)
            var chiTietHoaDons = donHang.ChiTietDonHangs.Select(ct => new ChiTietHoaDon
            {
                MaHoaDon = hoaDon.ID, // ID vừa sinh ra ở trên
                MaVatTu = ct.MaVatTu,
                SoLuong = ct.SoLuong,
                DonGia = ct.DonGia,
                ThanhTien = ct.DonGia * ct.SoLuong
            }).ToList();

            // Lưu lần 2
            _context.ChiTietHoaDons.AddRange(chiTietHoaDons);
            await _context.SaveChangesAsync();

            // Trả về thành công (Error = null) và ID mới
            return (null, hoaDon.ID);
        }

        public async Task<HoaDonDetailViewModel> GetInvoiceDetailAsync(int id)
        {
            var hd = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.VatTu)
                .FirstOrDefaultAsync(h => h.ID == id);

            if (hd == null) return null;

            return new HoaDonDetailViewModel
            {
                HoaDonId = hd.ID,
                MaHoaDon = $"HD{hd.ID:0000}",
                NgayLap = hd.NgayLap,
                TenKhachHang = hd.KhachHang.HoTen,
                DiaChiKhachHang = hd.KhachHang.DiaChi,
                PhuongThucThanhToan = hd.PhuongThucThanhToan,
                TongTienHang = hd.TongTienTruocThue,
                ThueGTGT = hd.TienThueGTGT ?? 0, // Trigger DB tính
                TongThanhToan = hd.TongTienSauThue ?? 0, // Trigger DB tính
                ChiTiet = hd.ChiTietHoaDons.Select((ct, index) => new HoaDonDetailViewModel.ChiTietItem
                {
                    STT = index + 1,
                    TenVatTu = ct.VatTu.TenVatTu,
                    DVT = ct.VatTu.DonViTinh,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                }).ToList()
            };
        }
    }
}