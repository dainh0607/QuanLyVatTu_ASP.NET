using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface IGioHangRepository : IGenericRepository<GioHang>
    {
        Task<GioHang?> GetByKhachHangIdAsync(int khachHangId);
    }
}
