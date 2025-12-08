using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface IDonHangRepository : IGenericRepository<DonHang>
    {
        Task<KhachHang> GetDonHangByKhachHangAsync(int khachHangId);

    }
}
