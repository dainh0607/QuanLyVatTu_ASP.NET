using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class VoucherExpiryJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VoucherExpiryJob> _logger;

        public VoucherExpiryJob(IServiceProvider serviceProvider, ILogger<VoucherExpiryJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("VoucherExpiryJob started.");

            // Chạy ngay sau 10s khi khởi động app để có thể test
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessVoucherExpirationsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi chạy VoucherExpiryJob");
                }

                // Run every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task ProcessVoucherExpirationsAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bắt đầu quét gửi thông báo Voucher sắp hết hạn...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var thongBaoService = scope.ServiceProvider.GetRequiredService<IThongBaoService>();

            var in3Days = DateTime.Now.Date.AddDays(3);
            var past7Days = DateTime.Now.Date.AddDays(-7);

            // Tìm những voucher trong ví KH còn "AVAILABLE" và sắp hết hạn (<= 3 ngày) HOẶC đã quá hạn gần đây
            var expiringWallets = await context.ViVoucherKhachHangs
                .Include(v => v.VoucherGoc)
                .Where(v => v.TrangThaiTrongVi == "AVAILABLE"
                         && v.VoucherGoc != null
                         && v.VoucherGoc.ThoiGianKetThuc.Date <= in3Days
                         && v.VoucherGoc.ThoiGianKetThuc.Date >= past7Days)
                .ToListAsync(stoppingToken);

            int count = 0;
            foreach (var vi in expiringWallets)
            {
                if (stoppingToken.IsCancellationRequested) break;

                if (vi.VoucherGoc == null) continue;
                var isExpired = vi.VoucherGoc.ThoiGianKetThuc.Date < DateTime.Now.Date;

                // Kiểm tra xem đã từng báo Voucher mã này cho KH này chưa (tránh spam rác)
                var alreadyNotified = await context.ThongBaos.AnyAsync(t =>
                    t.KhachHangId == vi.MaKhachHang
                    && t.LoaiThongBao == "Voucher"
                    && t.NoiDung.Contains(vi.VoucherGoc.MaVoucher)
                    && (isExpired ? t.TieuDe.Contains("đã hết") : t.TieuDe.Contains("sắp hết")), stoppingToken);

                if (!alreadyNotified)
                {
                    var title = isExpired ? "Voucher đã hết hạn!" : "Voucher sắp hết hạn!";
                    var message = isExpired 
                        ? $"Rất tiếc! Voucher mã {vi.VoucherGoc.MaVoucher} của bạn đã hết hạn vào ngày {vi.VoucherGoc.ThoiGianKetThuc:dd/MM/yyyy} và không thể sử dụng được nữa."
                        : $"Voucher mã {vi.VoucherGoc.MaVoucher} của bạn sắp hết hạn vào ngày {vi.VoucherGoc.ThoiGianKetThuc:dd/MM/yyyy}. Hãy nhanh chóng sử dụng trước khi mã ưu đãi bị thu hồi nhé!";

                    await thongBaoService.CreateVoucherNotificationAsync(
                        vi.MaKhachHang,
                        title,
                        message,
                        "/Customer/Profile#voucher"
                    );

                    // Nếu đã quá hạn thì update trạng thái luôn
                    if (isExpired)
                    {
                        vi.TrangThaiTrongVi = "EXPIRED";
                    }

                    count++;
                }
            }

            if (count > 0)
            {
                await context.SaveChangesAsync(stoppingToken);
            }

            _logger.LogInformation("Hoàn tất quét Voucher — Đã sinh {Count} thông báo nhắc nhở.", count);
        }
    }
}
