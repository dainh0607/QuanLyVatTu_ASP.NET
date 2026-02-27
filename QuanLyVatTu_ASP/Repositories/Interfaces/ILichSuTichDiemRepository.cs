using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface ILichSuTichDiemRepository : IGenericRepository<LichSuTichDiem>
    {
        /// <summary>
        /// Kiểm tra đã cộng điểm EARN cho đơn hàng chưa (Idempotency check)
        /// </summary>
        Task<bool> ExistsEarnForOrderAsync(int donHangId);

        /// <summary>
        /// Lấy lịch sử điểm của khách hàng
        /// </summary>
        Task<IEnumerable<LichSuTichDiem>> GetByKhachHangAsync(int khachHangId);

        /// <summary>
        /// Lấy giao dịch điểm theo đơn hàng
        /// </summary>
        Task<IEnumerable<LichSuTichDiem>> GetByDonHangAsync(int donHangId);
    }
}
