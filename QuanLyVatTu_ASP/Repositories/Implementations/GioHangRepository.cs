using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class GioHangRepository : GenericRepository<GioHang>, IGioHangRepository
    {
        private readonly AppDbContext _context;

        public GioHangRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<GioHang?> GetByKhachHangIdAsync(int khachHangId)
        {
            return await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .ThenInclude(ct => ct.VatTu)
                .FirstOrDefaultAsync(g => g.MaKhachHang == khachHangId);
        }
    }
}
