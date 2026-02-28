using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Models;

namespace QuanLyVatTu_ASP.Services.Interfaces
{
    public interface IVoucherService
    {
        /// <summary>
        /// Lưu mã voucher vào ví khách hàng
        /// </summary>
        Task<ServiceResult> SaveVoucherToWalletAsync(int khachHangId, int voucherId);

        /// <summary>
        /// Áp dụng voucher tại checkout — kiểm tra 3 điều kiện + Transaction
        /// Trả về số tiền được giảm
        /// </summary>
        Task<ServiceResult<decimal>> ApplyVoucherAsync(int khachHangId, int voucherId, int donHangId, decimal orderTotal);

        /// <summary>
        /// Xử lý voucher khi hủy đơn hàng (hoàn mã hoặc phạt mã)
        /// </summary>
        Task<ServiceResult> HandleOrderCancelVoucherAsync(int donHangId, string trangThaiDonHang);

        /// <summary>
        /// Lấy danh sách voucher trong ví khách hàng
        /// </summary>
        Task<IEnumerable<ViVoucherKhachHang>> GetWalletAsync(int khachHangId);

        /// <summary>
        /// Lấy danh sách voucher khả dụng (cho checkout)
        /// </summary>
        Task<IEnumerable<ViVoucherKhachHang>> GetAvailableForCheckoutAsync(int khachHangId);
    }
}
