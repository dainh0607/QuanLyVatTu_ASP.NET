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
            // 1. Build Query với Eager Loading cho navigation properties cần thiết
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
                query = query.Where(x => x.PhuongThucThanhToan == paymentMethod);
            }

            if (nhanVienId.HasValue && nhanVienId > 0)
            {
                query = query.Where(x => x.NhanVienId == nhanVienId);
            }

            if (khachHangId.HasValue && khachHangId > 0)
            {
                query = query.Where(x => x.KhachHangId == khachHangId);
            }

            // 3. Tính toán aggregate trực tiếp trên database (tối ưu hiệu suất)
            var totalRevenue = await query.SumAsync(x => x.TongTien ?? 0);
            var totalOrders = await query.CountAsync();
            var paidOrders = await query.CountAsync(x => x.TrangThai == "Hoàn thành");

            // 4. Chỉ load dữ liệu chi tiết cho danh sách hiển thị (có phân trang nếu cần)
            var rawData = await query
                .OrderByDescending(x => x.NgayDat)
                .Take(100) // Giới hạn số lượng để tránh load quá nhiều
                .ToListAsync();

            // 5. Build ViewModel
            var model = new DashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                PaidOrders = paidOrders,

                // Retain filter values for View
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                PaymentMethod = paymentMethod,
                NhanVienId = nhanVienId,
                KhachHangId = khachHangId
            };

            // 6. Map to List Items (sử dụng Lazy Loading cho navigation properties)
            model.Orders = rawData.Select(x => new OrderStatisticItem
            {
                Id = x.ID,
                MaDH = x.MaHienThi ?? $"DH{x.ID:0000}",
                Ngay = x.NgayDat,
                KhachHang = x.KhachHang?.HoTen ?? "Khách vãng lai",
                NhanVien = x.NhanVien?.HoTen ?? "Chưa phân công",
                TongTien = x.TongTien ?? 0,
                TrangThai = x.TrangThai ?? "Mới tạo"
            }).ToList();

            // 7. Process Chart Data (Group by Date) - tính trên dữ liệu đã load
            var chartGrouping = rawData
                .GroupBy(x => x.NgayDat.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    DateLabel = g.Key.ToString("MM-dd"),
                    DailyRevenue = g.Sum(x => x.TongTien ?? 0)
                })
                .ToList();

            model.ChartLabels = chartGrouping.Select(x => x.DateLabel).ToList();
            model.ChartData = chartGrouping.Select(x => x.DailyRevenue).ToList();

            // 8. Lấy dữ liệu Top 10 sản phẩm bán chạy dựa theo Doanh Thu (từ ChiTietDonHang áp dụng chung bộ lọc)
            var topProductsQuery = _context.ChiTietDonHangs
                .Include(c => c.DonHang)
                .Include(c => c.VatTu).ThenInclude(v => v.LoaiVatTu)
                .Include(c => c.VatTu).ThenInclude(v => v.NhaCungCap)
                .AsNoTracking()
                .AsQueryable();

            if (fromDate.HasValue) topProductsQuery = topProductsQuery.Where(x => x.DonHang.NgayDat.Date >= fromDate.Value.Date);
            if (toDate.HasValue) topProductsQuery = topProductsQuery.Where(x => x.DonHang.NgayDat.Date <= toDate.Value.Date);
            if (!string.IsNullOrEmpty(status) && status != "Tất cả") topProductsQuery = topProductsQuery.Where(x => x.DonHang.TrangThai == status);
            if (!string.IsNullOrEmpty(paymentMethod) && paymentMethod != "Tất cả") topProductsQuery = topProductsQuery.Where(x => x.DonHang.PhuongThucThanhToan == paymentMethod);
            if (nhanVienId.HasValue && nhanVienId > 0) topProductsQuery = topProductsQuery.Where(x => x.DonHang.NhanVienId == nhanVienId);
            if (khachHangId.HasValue && khachHangId > 0) topProductsQuery = topProductsQuery.Where(x => x.DonHang.KhachHangId == khachHangId);

            var topProductsData = await topProductsQuery
                .GroupBy(x => new { x.MaVatTu, x.VatTu.HinhAnh, x.VatTu.TenVatTu, x.VatTu.SoLuongTon, TenLoai = x.VatTu.LoaiVatTu != null ? x.VatTu.LoaiVatTu.TenLoaiVatTu : null, TenNcc = x.VatTu.NhaCungCap != null ? x.VatTu.NhaCungCap.TenNhaCungCap : null })
                .Select(g => new
                {
                    VatTuId = g.Key.MaVatTu,
                    HinhAnh = g.Key.HinhAnh,
                    TenVatTu = g.Key.TenVatTu,
                    LoaiVatTu = g.Key.TenLoai,
                    NhaCungCap = g.Key.TenNcc,
                    SoLuongTon = g.Key.SoLuongTon,
                    SoLuongBan = g.Sum(x => x.SoLuong ?? 0),
                    DoanhThu = g.Sum(x => x.ThanhTien)
                })
                .OrderByDescending(x => x.DoanhThu)
                .Take(10)
                .ToListAsync();

            model.TopProducts = topProductsData.Select(x => new ProductStatisticItem
            {
                VatTuId = x.VatTuId,
                HinhAnh = x.HinhAnh ?? string.Empty,
                LoaiVatTu = x.LoaiVatTu ?? string.Empty,
                TenVatTu = x.TenVatTu ?? string.Empty,
                SoLuongTon = x.SoLuongTon ?? 0,
                NhaCungCap = x.NhaCungCap ?? string.Empty,
                SoLuongBan = x.SoLuongBan,
                DoanhThu = x.DoanhThu
            }).ToList();

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
                "Chờ xác nhận", "Đã xác nhận", "Đang xử lý", "Đang giao hàng", "Hoàn thành", "Đã hủy"
            };

            var phuongThucs = new List<string> {
                "Tiền mặt", "Chuyển khoản", "COD"
            };

            return (nhanVienList, khachHangList, trangThais, phuongThucs);
        }
    }
}