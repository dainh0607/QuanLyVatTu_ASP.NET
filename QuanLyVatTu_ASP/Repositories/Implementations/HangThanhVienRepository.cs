using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class HangThanhVienRepository : GenericRepository<HangThanhVien>, IHangThanhVienRepository
    {
        public HangThanhVienRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<HangThanhVien>> GetAllOrderedAsync()
        {
            return await _context.HangThanhViens
                .OrderBy(h => h.ChiTieuToiThieu)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<HangThanhVien?> GetTierForSpentAsync(decimal totalSpent)
        {
            // Lấy hạng cao nhất mà khách đạt đủ mức chi tiêu
            return await _context.HangThanhViens
                .Where(h => h.ChiTieuToiThieu <= totalSpent)
                .OrderByDescending(h => h.ChiTieuToiThieu)
                .FirstOrDefaultAsync();
        }
    }
}
