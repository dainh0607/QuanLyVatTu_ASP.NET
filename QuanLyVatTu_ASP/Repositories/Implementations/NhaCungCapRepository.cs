using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class NhaCungCapRepository : GenericRepository<NhaCungCap>, INhaCungCapRepository
    {
        public NhaCungCapRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
