using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class YeuThichRepository : GenericRepository<YeuThich>, IYeuThichRepository
    {
        public YeuThichRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<YeuThich>> GetByKhachHangIdAsync(int khachHangId)
        {
            return await _dbSet
                .Where(x => x.MaKhachHang == khachHangId)
                .Include(x => x.VatTu) // Eager load VatTu info
                .OrderByDescending(x => x.NgayThem)
                .ToListAsync();
        }

        public async Task<YeuThich?> GetByKhachHangAndVatTuAsync(int khachHangId, int vatTuId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.MaKhachHang == khachHangId && x.MaVatTu == vatTuId);
        }
    }
}
