using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class HoaDonService : IHoaDonService
    {
        private readonly AppDbContext _context;

        public HoaDonService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HoaDonViewModel> GetOrdersForIndexAsync(string keyword, int page, int pageSize)
        {
            if (page < 1) page = 1;
            
            var query = _context.DonHang
                .Include(d => d.KhachHang)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(d => 
                    (d.MaHienThi != null && d.MaHienThi.ToLower().Contains(keyword)) ||
                    (d.KhachHang != null && d.KhachHang.HoTen.ToLower().Contains(keyword)));
            }
            
            var totalRecords = await query.CountAsync();

            var orders = await query
                .OrderByDescending(d => d.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new HoaDonViewModel.OrderForItem
                {
                    DonHangId = d.ID,
                    MaDonHang = d.MaHienThi ?? $"DH{d.ID:0000}",
                    TenKhachHang = d.KhachHang != null ? d.KhachHang.HoTen : "",
                    NgayDat = d.NgayDat,
                    TongTien = d.TongTien ?? 0,
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

            return new HoaDonViewModel 
            { 
                Orders = orders,
                PageIndex = page,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<(string? Error, int NewInvoiceId)> CreateInvoiceFromOrderAsync(int donHangId)
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
            decimal tyLe = (donHang.TongTien ?? 0) > 0 ? ((donHang.SoTienDatCoc ?? 0) / (donHang.TongTien ?? 0)) : 0;
            if (tyLe < 0.1m)
            {
                return ($"Chưa đủ điều kiện! Khách mới cọc {tyLe:P0}. Cần tối thiểu 10%.", 0);
            }

            // 4. Tạo Header Hóa đơn
            var hoaDon = new HoaDon
            {
                MaDonHang = donHang.ID,
                MaNhanVien = donHang.NhanVienId ?? 1, // Mặc định 1 nếu null (cần cẩn thận chỗ này tùy data thật)
                MaKhachHang = donHang.KhachHangId,
                NgayLap = DateTime.Now,
                TongTienTruocThue = donHang.TongTien ?? 0,
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
                SoLuong = ct.SoLuong ?? 0,
                DonGia = ct.DonGia ?? 0,
                ThanhTien = (ct.DonGia ?? 0) * (ct.SoLuong ?? 0)
            }).ToList();

            // Lưu lần 2
            _context.ChiTietHoaDons.AddRange(chiTietHoaDons);
            await _context.SaveChangesAsync();

            // Trả về thành công (Error = null) và ID mới
            return (null, hoaDon.ID);
        }

        public async Task<HoaDonDetailViewModel?> GetInvoiceDetailAsync(int id)
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
                TenKhachHang = hd.KhachHang?.HoTen ?? "",
                DiaChiKhachHang = hd.KhachHang?.DiaChi,
                PhuongThucThanhToan = hd.PhuongThucThanhToan,
                TongTienHang = hd.TongTienTruocThue ?? 0,
                ThueGTGT = hd.TienThueGTGT ?? 0, // Trigger DB tính
                TongThanhToan = hd.TongTienSauThue ?? 0, // Trigger DB tính
                ChiTiet = hd.ChiTietHoaDons.Select((ct, index) => new HoaDonDetailViewModel.ChiTietItem
                {
                    STT = index + 1,
                    TenVatTu = ct.VatTu.TenVatTu,
                    DVT = ct.VatTu.DonViTinh,
                    SoLuong = ct.SoLuong ?? 0,
                    DonGia = ct.DonGia ?? 0,
                    ThanhTien = (ct.SoLuong ?? 0) * (ct.DonGia ?? 0)
                }).ToList()
            };
        }

                public void CalculateHoaDon(HoaDon hoaDon)
        {
            if (hoaDon == null) return;

            // 1. TienThueGTGT
            // Logic: (TongTienTruocThue * TyLeThueGTGT) / 100
            // Round 2, AwayFromZero
            decimal taxAmount = ((hoaDon.TongTienTruocThue ?? 0) * hoaDon.TyLeThueGTGT) / 100m;
            hoaDon.TienThueGTGT = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero);

            // 2. TongTienSauThue
            // Logic: TongTienTruocThue + TienThueGTGT - ChietKhau
            // Note: Use the calculated TienThueGTGT or recalculate? 
            // Usually simpler to sum components.
            // Prompt formula: TongTienTruocThue + (TongTienTruocThue * TyLeThueGTGT / 100) - (ChietKhau ?? 0)
            
            decimal calculatedTotal = (hoaDon.TongTienTruocThue ?? 0) + (hoaDon.TienThueGTGT ?? 0) - (hoaDon.ChietKhau ?? 0);
            hoaDon.TongTienSauThue = Math.Round(calculatedTotal, 2, MidpointRounding.AwayFromZero);
        }
    }
}