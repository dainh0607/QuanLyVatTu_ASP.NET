using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface IVatTuRepository : IGenericRepository<VatTu>
    {
        IEnumerable<VatTu> GetVatTuKemLoai();
        Task<VatTu?> GetByIdRealtimeAsync(int id);
    }
}
