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
            _logger.LogInformation("TierDowngradeJob started. Will run an initial check in 15 seconds.");

            // Chạy ngay lúc khởi động để dễ test
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessTierDowngradesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi chạy TierDowngradeJob");
                }

                // Tính thời gian đến 00:00 ngày hôm sau
                var now = DateTime.Now;
                var nextMidnight = now.Date.AddDays(1); // 00:00 ngày mai
                var delay = nextMidnight - now;

                _logger.LogInformation("TierDowngradeJob sẽ delay đến {NextRun} (sau {Delay})", nextMidnight, delay);
                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task ProcessTierDowngradesAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bắt đầu quét rớt hạng thành viên...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var today = DateTime.Now.Date;
            var oneYearAgo = today.AddDays(-365);
            var in7Days = today.AddDays(7);
            var in3Days = today.AddDays(3);

            // Bổ sung: Lấy khách hàng hết hạn HÔM NAY (để Rớt Hạng) VÀ Hết hạn trong 7 ngày / 3 ngày (để Cảnh báo)
            var expiringCustomers = await context.KhachHangs
                .Where(kh => kh.NgayHetHanHang != null
                          && (kh.NgayHetHanHang.Value.Date == today 
                              || kh.NgayHetHanHang.Value.Date == in7Days 
                              || kh.NgayHetHanHang.Value.Date == in3Days)
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
                             && (d.TrangThai == "Đã giao" || d.TrangThai == "Đã thanh toán" || d.TrangThai == "Hoàn thành")
                             && d.NgayDat >= oneYearAgo)
                    .SumAsync(d => d.TongTienThucTra ?? d.TongTien ?? 0, stoppingToken);

                if (khachHang.HangThanhVien != null && totalSpent >= khachHang.HangThanhVien.ChiTieuToiThieu)
                {
                    // NẾU HÔM NAY HẾT HẠN MÀ ĐẠT ĐỦ CHỈ TIÊU → Gia hạn thêm 1 năm
                    if (khachHang.NgayHetHanHang.Value.Date == today)
                    {
                        khachHang.NgayLenHang = DateTime.Now;
                        khachHang.NgayHetHanHang = DateTime.Now.AddYears(1);
                        _logger.LogInformation("Khách hàng {Id} ({Name}) — GIA HẠN hạng {Tier} thêm 1 năm.",
                            khachHang.ID, khachHang.HoTen, khachHang.HangThanhVien.TenHang);

                        var thongBaoService = scope.ServiceProvider.GetRequiredService<QuanLyVatTu_ASP.Services.Interfaces.IThongBaoService>();
                        await thongBaoService.CreateTierNotificationAsync(
                            khachHang.ID,
                            "Gia hạn hạng thành viên thành công!",
                            $"Chúc mừng bạn đã duy trì đủ mức chi tiêu để gia hạn hạng {khachHang.HangThanhVien.TenHang} thêm 1 năm. Tiếp tục mua sắm để nhận nhiều ưu đãi nhé!",
                            "/Customer/Profile#points"
                        );
                    }
                    else
                    {
                        // Sắp hết hạn nhưng đã ráng tiêu đủ rồi thì thôi không báo "Cảnh báo" nữa.
                        continue;
                    }
                }
                else
                {
                    // CHƯA ĐẠT CHỈ TIÊU HOẶC HẾT HẠN
                    if (khachHang.NgayHetHanHang.Value.Date == today)
                    {
                        // KHÔNG ĐẠT MÀ CÒN ĐÚNG NGÀY HÔM NAY -> Rớt hạng
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

                        var thongBaoService = scope.ServiceProvider.GetRequiredService<QuanLyVatTu_ASP.Services.Interfaces.IThongBaoService>();
                        await thongBaoService.CreateTierNotificationAsync(
                            khachHang.ID,
                            "Hạng thành viên của bạn đã thay đổi",
                            $"Do không đạt đủ chỉ tiêu chi tiêu mua sắm, hạng thành viên của bạn đã bị giảm từ {oldTierName} xuống {suitableTier.TenHang}.",
                            "/Customer/Profile#points"
                        );
                    }
                    else
                    {
                        // Rớt về hạng cơ bản (không có ngày hết hạn)
                        khachHang.MaHangThanhVien = basicTier?.ID;
                        khachHang.NgayLenHang = null;
                        khachHang.NgayHetHanHang = null;
                        _logger.LogInformation("Khách hàng {Id} ({Name}) — RỚT HẠNG từ {OldTier} về Cơ bản.",
                            khachHang.ID, khachHang.HoTen, oldTierName);

                        var thongBaoService = scope.ServiceProvider.GetRequiredService<QuanLyVatTu_ASP.Services.Interfaces.IThongBaoService>();
                        await thongBaoService.CreateTierNotificationAsync(
                            khachHang.ID,
                            "Bạn đã trở về hạng Cơ bản",
                            $"Thật tiếc! Định mức chi tiêu của bạn đã hết hạn và bạn đã bị rớt khỏi hạng {oldTierName}. Hãy tiếp tục mua sắm để thăng hạng nhé!",
                            "/Customer/Profile#points"
                        );
                    }
                    }
                    else if (khachHang.NgayHetHanHang.Value.Date == in7Days || khachHang.NgayHetHanHang.Value.Date == in3Days)
                    {
                        var daysLeft = (khachHang.NgayHetHanHang.Value.Date - today).Days;
                        var needToSpend = (khachHang.HangThanhVien?.ChiTieuToiThieu ?? 0) - totalSpent;
                        if (needToSpend > 0)
                        {
                            var thongBaoService = scope.ServiceProvider.GetRequiredService<QuanLyVatTu_ASP.Services.Interfaces.IThongBaoService>();
                            await thongBaoService.CreateTierNotificationAsync(
                                khachHang.ID,
                                $"Cảnh báo: Hạng {khachHang.HangThanhVien?.TenHang} của bạn sắp hết hạn!",
                                $"Thứ hạng của bạng chỉ còn {daysLeft} ngày là hết hạn trình duy trì. Bạn cần mua sắm và chi tiêu thêm {needToSpend:N0}đ nữa để giữ vững hạng {khachHang.HangThanhVien?.TenHang}.",
                                "/Customer/Profile#points"
                            );
                        }
                    }
                }
            }

            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Hoàn tất quét hạng — Đã xử lý {Count} khách hàng.", expiringCustomers.Count);
        }
    }
}
