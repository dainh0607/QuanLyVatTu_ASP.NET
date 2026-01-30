using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.ThongKe;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class ThongKeService : IThongKeService
    {
        private readonly AppDbContext _context;

        public ThongKeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardStatsAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? status,
            string? paymentMethod,
            int? nhanVienId,
            int? khachHangId)
        {
            // 1. Build Query
            var query = _context.DonHang
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien)
                .AsNoTracking()
                .AsQueryable();

            // 2. Apply Filters
            if (fromDate.HasValue)
            {
                query = query.Where(x => x.NgayDat.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.NgayDat.Date <= toDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                query = query.Where(x => x.TrangThai == status);
            }

            if (!string.IsNullOrEmpty(paymentMethod) && paymentMethod != "Tất cả")
            {
                query = query.Where(x => x.PhuongThucDatCoc == paymentMethod);
            }

            if (nhanVienId.HasValue && nhanVienId > 0)
            {
                query = query.Where(x => x.NhanVienId == nhanVienId);
            }

            if (khachHangId.HasValue && khachHangId > 0)
            {
                query = query.Where(x => x.KhachHangId == khachHangId);
            }

            // 3. Execute Query
            var rawData = await query.OrderByDescending(x => x.NgayDat).ToListAsync();

            // 4. Calculate Stats
            var model = new DashboardViewModel
            {
                TotalRevenue = rawData.Sum(x => x.TongTien),
                TotalOrders = rawData.Count,
                PaidOrders = rawData.Count(x => x.TrangThai == "Hoàn thành" || x.TrangThai == "Đã thanh toán"),

                // Retain filter values for View
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                PaymentMethod = paymentMethod,
                NhanVienId = nhanVienId,
                KhachHangId = khachHangId
            };

            // 5. Map to List Items
            model.Orders = rawData.Select(x => new OrderStatisticItem
            {
                Id = x.ID,
                MaDH = x.MaHienThi ?? $"DH{x.ID:0000}",
                Ngay = x.NgayDat,
                KhachHang = x.KhachHang?.HoTen ?? "Khách vãng lai",
                NhanVien = x.NhanVien?.HoTen ?? "Chưa phân công",
                TongTien = x.TongTien,
                TrangThai = x.TrangThai ?? "Mới tạo"
            }).ToList();

            // 6. Process Chart Data (Group by Date)
            var chartGrouping = rawData
                .GroupBy(x => x.NgayDat.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    DateLabel = g.Key.ToString("MM-dd"),
                    DailyRevenue = g.Sum(x => x.TongTien)
                })
                .ToList();

            model.ChartLabels = chartGrouping.Select(x => x.DateLabel).ToList();
            model.ChartData = chartGrouping.Select(x => x.DailyRevenue).ToList();

            return model;
        }

        public async Task<(SelectList NhanViens, SelectList KhachHangs, List<string> TrangThais, List<string> PhuongThucs)> GetFilterDropdownsAsync(int? selectedNhanVien, int? selectedKhachHang)
        {
            var nhanViens = await _context.NhanViens
                .Select(x => new { x.ID, x.HoTen })
                .ToListAsync();
            var nhanVienList = new SelectList(nhanViens, "ID", "HoTen", selectedNhanVien);

            var khachHangs = await _context.KhachHangs
                .Select(x => new { x.ID, x.HoTen })
                .ToListAsync();
            var khachHangList = new SelectList(khachHangs, "ID", "HoTen", selectedKhachHang);

            var trangThais = new List<string> {
                "Chờ xác nhận", "Đã xác nhận", "Đang xử lý", "Hoàn thành", "Đã hủy", "Đã thanh toán", "Chưa thanh toán"
            };

            var phuongThucs = new List<string> {
                "Tiền mặt", "Chuyển khoản", "COD"
            };

            return (nhanVienList, khachHangList, trangThais, phuongThucs);
        }
    }
}