using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Models;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class DiemTichLuyService : IDiemTichLuyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public DiemTichLuyService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        // ==========================================
        // 1. Tích điểm (EARN) — Gọi khi đơn "Đã giao thành công"
        // ==========================================
        public async Task<ServiceResult> EarnPointsAsync(int khachHangId, int donHangId, decimal finalAmount)
        {
            // Idempotency: Kiểm tra đã cộng điểm cho đơn này chưa
            var alreadyEarned = await _unitOfWork.LichSuTichDiemRepository.ExistsEarnForOrderAsync(donHangId);
            if (alreadyEarned)
                return ServiceResult.Ok("Điểm đã được cộng cho đơn hàng này.");

            // Tính điểm: 1% giá trị thanh toán cuối cùng, làm tròn xuống
            int soDiem = (int)Math.Floor(finalAmount * 0.01m);
            if (soDiem <= 0)
                return ServiceResult.Ok("Giá trị đơn hàng quá nhỏ để tích điểm.");

            // Insert lịch sử
            var lichSu = new LichSuTichDiem
            {
                MaKhachHang = khachHangId,
                MaDonHang = donHangId,
                SoDiem = soDiem,
                LoaiGiaoDich = "EARN"
            };
            await _unitOfWork.LichSuTichDiemRepository.AddAsync(lichSu);

            // Cộng vào current_points
            var khachHang = await _context.KhachHangs.FindAsync(khachHangId);
            if (khachHang != null)
            {
                khachHang.DiemTichLuy += soDiem;
            }

            await _unitOfWork.SaveAsync();
            return ServiceResult.Ok($"Đã cộng {soDiem:N0} điểm vào tài khoản.");
        }

        // ==========================================
        // 2. Tiêu điểm (REDEEM) — Transaction + Row Lock
        // ==========================================
        public async Task<ServiceResult> RedeemPointsAsync(int khachHangId, int donHangId, int points)
        {
            if (points <= 0)
                return ServiceResult.Fail("Số điểm phải lớn hơn 0.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lock row KhachHang để chống gian lận multi-tab
                var khachHang = await _context.KhachHangs
                    .FromSqlRaw("SELECT * FROM KhachHang WITH (UPDLOCK, ROWLOCK) WHERE ID = {0}", khachHangId)
                    .FirstOrDefaultAsync();

                if (khachHang == null)
                    return ServiceResult.Fail("Không tìm thấy khách hàng.");

                // Kiểm tra số dư
                if (khachHang.DiemTichLuy < points)
                    return ServiceResult.Fail($"Số dư điểm không đủ. Bạn có {khachHang.DiemTichLuy:N0} điểm.");

                // Trừ điểm tức thì
                khachHang.DiemTichLuy -= points;

                // Insert lịch sử REDEEM
                var lichSu = new LichSuTichDiem
                {
                    MaKhachHang = khachHangId,
                    MaDonHang = donHangId,
                    SoDiem = points,
                    LoaiGiaoDich = "REDEEM"
                };
                await _context.LichSuTichDiems.AddAsync(lichSu);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Ok($"Đã sử dụng {points:N0} điểm, giảm {points:N0}₫.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Fail("Lỗi khi sử dụng điểm: " + ex.Message);
            }
        }

        // ==========================================
        // 3. Hoàn điểm (REFUND) — Khi hủy đơn
        // ==========================================
        public async Task<ServiceResult> RefundPointsAsync(int donHangId)
        {
            var pointTransactions = await _unitOfWork.LichSuTichDiemRepository.GetByDonHangAsync(donHangId);
            var redeemRecord = pointTransactions.FirstOrDefault(p => p.LoaiGiaoDich == "REDEEM");

            if (redeemRecord == null)
                return ServiceResult.Ok("Đơn hàng không sử dụng điểm.");

            // Kiểm tra đã refund chưa
            var existingRefund = pointTransactions.FirstOrDefault(p => p.LoaiGiaoDich == "REFUND");
            if (existingRefund != null)
                return ServiceResult.Ok("Điểm đã được hoàn trả trước đó.");

            // Hoàn 100% điểm đã dùng
            var khachHang = await _context.KhachHangs.FindAsync(redeemRecord.MaKhachHang);
            if (khachHang != null)
            {
                khachHang.DiemTichLuy += redeemRecord.SoDiem;
            }

            // Insert lịch sử REFUND
            var refundRecord = new LichSuTichDiem
            {
                MaKhachHang = redeemRecord.MaKhachHang,
                MaDonHang = donHangId,
                SoDiem = redeemRecord.SoDiem,
                LoaiGiaoDich = "REFUND"
            };
            await _unitOfWork.LichSuTichDiemRepository.AddAsync(refundRecord);
            await _unitOfWork.SaveAsync();

            return ServiceResult.Ok($"Đã hoàn trả {redeemRecord.SoDiem:N0} điểm.");
        }

        // ==========================================
        // 4. Thu hồi điểm (CLAWBACK) — Khi trả hàng
        // ==========================================
        public async Task<ServiceResult> ClawbackPointsAsync(int donHangId)
        {
            var pointTransactions = await _unitOfWork.LichSuTichDiemRepository.GetByDonHangAsync(donHangId);
            var earnRecord = pointTransactions.FirstOrDefault(p => p.LoaiGiaoDich == "EARN");

            if (earnRecord == null)
                return ServiceResult.Ok("Đơn hàng chưa được cộng điểm.");

            // Kiểm tra đã clawback chưa
            var existingClawback = pointTransactions.FirstOrDefault(p => p.LoaiGiaoDich == "CLAWBACK");
            if (existingClawback != null)
                return ServiceResult.Ok("Điểm đã được thu hồi trước đó.");

            // Thu hồi điểm — CHẤP NHẬN ĐIỂM ÂM
            var khachHang = await _context.KhachHangs.FindAsync(earnRecord.MaKhachHang);
            if (khachHang != null)
            {
                khachHang.DiemTichLuy -= earnRecord.SoDiem;
                // Có thể xuống âm, đúng theo yêu cầu nghiệp vụ
            }

            // Insert lịch sử CLAWBACK
            var clawbackRecord = new LichSuTichDiem
            {
                MaKhachHang = earnRecord.MaKhachHang,
                MaDonHang = donHangId,
                SoDiem = earnRecord.SoDiem,
                LoaiGiaoDich = "CLAWBACK"
            };
            await _unitOfWork.LichSuTichDiemRepository.AddAsync(clawbackRecord);
            await _unitOfWork.SaveAsync();

            return ServiceResult.Ok($"Đã thu hồi {earnRecord.SoDiem:N0} điểm.");
        }

        // ==========================================
        // 5. Thăng hạng (Tier Upgrade)
        // ==========================================
        public async Task EvaluateTierUpgradeAsync(int khachHangId)
        {
            // Tính tổng chi tiêu thực tế 365 ngày gần nhất
            var oneYearAgo = DateTime.Now.AddDays(-365);
            var totalSpent = await _context.DonHang
                .Where(d => d.KhachHangId == khachHangId
                         && d.TrangThai == "Đã giao"
                         && d.NgayDat >= oneYearAgo)
                .SumAsync(d => d.TongTienThucTra ?? d.TongTien ?? 0);

            // Tìm hạng cao nhất phù hợp
            var tier = await _unitOfWork.HangThanhVienRepository.GetTierForSpentAsync(totalSpent);
            if (tier == null) return;

            var khachHang = await _context.KhachHangs.FindAsync(khachHangId);
            if (khachHang == null) return;

            // Chỉ nâng hạng nếu hạng mới cao hơn hiện tại
            if (khachHang.MaHangThanhVien == null || tier.ChiTieuToiThieu > 0)
            {
                var currentTier = khachHang.MaHangThanhVien.HasValue
                    ? await _unitOfWork.HangThanhVienRepository.GetByIdAsync(khachHang.MaHangThanhVien.Value)
                    : null;

                if (currentTier == null || tier.ChiTieuToiThieu > currentTier.ChiTieuToiThieu)
                {
                    khachHang.MaHangThanhVien = tier.ID;
                    khachHang.NgayLenHang = DateTime.Now;
                    khachHang.NgayHetHanHang = DateTime.Now.AddYears(1);
                    await _unitOfWork.SaveAsync();
                }
            }
        }

        // ==========================================
        // 6. Lấy lịch sử giao dịch điểm
        // ==========================================
        public async Task<IEnumerable<LichSuTichDiem>> GetHistoryAsync(int khachHangId)
        {
            return await _unitOfWork.LichSuTichDiemRepository.GetByKhachHangAsync(khachHangId);
        }
    }
}
