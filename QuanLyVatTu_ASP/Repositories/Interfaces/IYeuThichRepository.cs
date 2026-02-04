using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface IYeuThichRepository : IGenericRepository<YeuThich>
    {
        Task<IEnumerable<YeuThich>> GetByKhachHangIdAsync(int khachHangId);
        Task<YeuThich?> GetByKhachHangAndVatTuAsync(int khachHangId, int vatTuId);
    }
}
