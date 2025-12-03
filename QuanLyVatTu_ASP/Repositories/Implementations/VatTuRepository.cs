using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class VatTuRepository : GenericRepository<VatTu>, IVatTuRepository
    {
        public VatTuRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IEnumerable<VatTu> GetVatTuKemLoai()
        {
            return _dbSet.Include(v => v.LoaiVatTu).ToList();
        }
    }
}
