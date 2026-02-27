using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Models;

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface IDiemTichLuyService
    {
        /// <summary>
        /// Cộng điểm khi đơn hàng "Đã giao thành công" (1% tổng tiền thực trả)
        /// </summary>
        Task<ServiceResult> EarnPointsAsync(int khachHangId, int donHangId, decimal finalAmount);

        /// <summary>
        /// Trừ điểm tức thì khi khách chọn dùng điểm tại checkout (Transaction + Row Lock)
        /// </summary>
        Task<ServiceResult> RedeemPointsAsync(int khachHangId, int donHangId, int points);

        /// <summary>
        /// Hoàn điểm đã REDEEM khi hủy đơn
        /// </summary>
        Task<ServiceResult> RefundPointsAsync(int donHangId);

        /// <summary>
        /// Thu hồi điểm EARN khi trả hàng (chấp nhận điểm âm)
        /// </summary>
        Task<ServiceResult> ClawbackPointsAsync(int donHangId);

        /// <summary>
        /// Đánh giá nâng hạng sau khi đơn hàng giao thành công
        /// </summary>
        Task EvaluateTierUpgradeAsync(int khachHangId);

        /// <summary>
        /// Lấy lịch sử giao dịch điểm của khách hàng
        /// </summary>
        Task<IEnumerable<LichSuTichDiem>> GetHistoryAsync(int khachHangId);
    }
}
