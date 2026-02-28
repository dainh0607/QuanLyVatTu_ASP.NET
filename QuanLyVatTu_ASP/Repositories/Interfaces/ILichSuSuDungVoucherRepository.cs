using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface ILichSuSuDungVoucherRepository : IGenericRepository<LichSuSuDungVoucher>
    {
        /// <summary>
        /// Đếm số lượt đã dùng (APPLIED + BURNED) của 1 khách hàng với 1 voucher
        /// </summary>
        Task<int> CountUsageAsync(int khachHangId, int voucherId);

        /// <summary>
        /// Lấy lịch sử sử dụng voucher theo đơn hàng
        /// </summary>
        Task<LichSuSuDungVoucher?> GetByDonHangAsync(int donHangId);
    }
}
