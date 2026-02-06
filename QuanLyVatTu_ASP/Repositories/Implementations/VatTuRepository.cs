using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class VatTuRepository : GenericRepository<VatTu>, IVatTuRepository
    {
        public VatTuRepository(AppDbContext context) : base(context)
        {
        }
        public IEnumerable<VatTu> GetVatTuKemLoai()
        {
            return _context.VatTus
                .Include(v => v.LoaiVatTu)
                .Include(v => v.NhaCungCap)
                .ToList();
        }

        public async Task<VatTu?> GetByIdRealtimeAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                await _context.Entry(entity).ReloadAsync();
            }
            return entity;
        }
    }
}