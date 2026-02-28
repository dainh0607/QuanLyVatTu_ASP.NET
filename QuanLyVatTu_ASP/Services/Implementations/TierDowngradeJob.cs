using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.DataAccess;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    /// <summary>
    /// Background Job chạy hàng đêm lúc 00:00 để quét rớt hạng thành viên.
    /// Kiểm tra các khách hàng có NgayHetHanHang = hôm nay,
    /// tính lại tổng chi tiêu 365 ngày → Giữ hạng hoặc Rớt hạng.
    /// </summary>
    public class TierDowngradeJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TierDowngradeJob> _logger;

        public TierDowngradeJob(IServiceProvider serviceProvider, ILogger<TierDowngradeJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TierDowngradeJob started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Tính thời gian đến 00:00 ngày hôm sau
                var now = DateTime.Now;
                var nextMidnight = now.Date.AddDays(1); // 00:00 ngày mai
                var delay = nextMidnight - now;

                _logger.LogInformation("TierDowngradeJob sẽ chạy lúc {NextRun} (sau {Delay})", nextMidnight, delay);
                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    await ProcessTierDowngradesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi chạy TierDowngradeJob");
                }
            }
        }

        private async Task ProcessTierDowngradesAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bắt đầu quét rớt hạng thành viên...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var today = DateTime.Now.Date;
            var oneYearAgo = today.AddDays(-365);

            // Lấy DS khách hàng có ngày hết hạn hạng = hôm nay
            var expiringCustomers = await context.KhachHangs
                .Where(kh => kh.NgayHetHanHang != null
                          && kh.NgayHetHanHang.Value.Date <= today
                          && kh.MaHangThanhVien != null)
                .Include(kh => kh.HangThanhVien)
                .ToListAsync(stoppingToken);

            if (!expiringCustomers.Any())
            {
                _logger.LogInformation("Không có khách hàng nào cần xét hạng hôm nay.");
                return;
            }

            // Lấy hạng cơ bản (hạng thấp nhất, ChiTieuToiThieu = 0)
            var basicTier = await context.HangThanhViens
                .OrderBy(h => h.ChiTieuToiThieu)
                .FirstOrDefaultAsync(stoppingToken);

            foreach (var khachHang in expiringCustomers)
            {
                if (stoppingToken.IsCancellationRequested) break;

                // Tính tổng chi tiêu 365 ngày qua
                var totalSpent = await context.DonHang
                    .Where(d => d.KhachHangId == khachHang.ID
                             && d.TrangThai == "Đã giao"
                             && d.NgayDat >= oneYearAgo)
                    .SumAsync(d => d.TongTienThucTra ?? d.TongTien ?? 0, stoppingToken);

                if (khachHang.HangThanhVien != null && totalSpent >= khachHang.HangThanhVien.ChiTieuToiThieu)
                {
                    // ĐẠT ĐỦ CHỈ TIÊU → Gia hạn thêm 1 năm
                    khachHang.NgayLenHang = DateTime.Now;
                    khachHang.NgayHetHanHang = DateTime.Now.AddYears(1);
                    _logger.LogInformation("Khách hàng {Id} ({Name}) — GIA HẠN hạng {Tier} thêm 1 năm.",
                        khachHang.ID, khachHang.HoTen, khachHang.HangThanhVien.TenHang);
                }
                else
                {
                    // KHÔNG ĐẠT → Rớt hạng về cơ bản
                    var oldTierName = khachHang.HangThanhVien?.TenHang ?? "N/A";
                    
                    // Tìm hạng phù hợp nhất dựa trên chi tiêu hiện tại
                    var suitableTier = await context.HangThanhViens
                        .Where(h => h.ChiTieuToiThieu <= totalSpent)
                        .OrderByDescending(h => h.ChiTieuToiThieu)
                        .FirstOrDefaultAsync(stoppingToken);

                    if (suitableTier != null && suitableTier.ChiTieuToiThieu > 0)
                    {
                        // Giảm xuống hạng phù hợp (không phải cơ bản)
                        khachHang.MaHangThanhVien = suitableTier.ID;
                        khachHang.NgayLenHang = DateTime.Now;
                        khachHang.NgayHetHanHang = DateTime.Now.AddYears(1);
                        _logger.LogInformation("Khách hàng {Id} ({Name}) — GIẢM từ {OldTier} xuống {NewTier}.",
                            khachHang.ID, khachHang.HoTen, oldTierName, suitableTier.TenHang);
                    }
                    else
                    {
                        // Rớt về hạng cơ bản (không có ngày hết hạn)
                        khachHang.MaHangThanhVien = basicTier?.ID;
                        khachHang.NgayLenHang = null;
                        khachHang.NgayHetHanHang = null;
                        _logger.LogInformation("Khách hàng {Id} ({Name}) — RỚT HẠNG từ {OldTier} về Cơ bản.",
                            khachHang.ID, khachHang.HoTen, oldTierName);
                    }
                }
            }

            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Hoàn tất quét hạng — Đã xử lý {Count} khách hàng.", expiringCustomers.Count);
        }
    }
}
