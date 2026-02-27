using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface IViVoucherRepository : IGenericRepository<ViVoucherKhachHang>
    {
        Task<IEnumerable<ViVoucherKhachHang>> GetByKhachHangAsync(int khachHangId);
        Task<bool> ExistsAsync(int khachHangId, int voucherId);
        Task<IEnumerable<ViVoucherKhachHang>> GetAvailableAsync(int khachHangId);
        Task<ViVoucherKhachHang?> GetByKhachHangAndVoucherAsync(int khachHangId, int voucherId);
    }
}
