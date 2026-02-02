using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface INhanVienRepository : IGenericRepository<NhanVien>
    {
        NhanVien? GetByLogin(string email, string password);
    }
}
