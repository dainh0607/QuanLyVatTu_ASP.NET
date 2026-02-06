using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface IDonHangRepository : IGenericRepository<DonHang>
    {
        Task<IEnumerable<DonHang>> GetDonHangByKhachHangAsync(int khachHangId);
        Task<DonHang?> GetDonHangByIdAsync(int id);
    }
}
