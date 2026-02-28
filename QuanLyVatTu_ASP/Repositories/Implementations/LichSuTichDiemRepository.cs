using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class LichSuTichDiemRepository : GenericRepository<LichSuTichDiem>, ILichSuTichDiemRepository
    {
        public LichSuTichDiemRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsEarnForOrderAsync(int donHangId)
        {
            return await _context.LichSuTichDiems
                .AnyAsync(l => l.MaDonHang == donHangId && l.LoaiGiaoDich == "EARN");
        }

        public async Task<IEnumerable<LichSuTichDiem>> GetByKhachHangAsync(int khachHangId)
        {
            return await _context.LichSuTichDiems
                .Include(l => l.DonHang)
                .Where(l => l.MaKhachHang == khachHangId)
                .OrderByDescending(l => l.NgayTao)
                .ToListAsync();
        }

        public async Task<IEnumerable<LichSuTichDiem>> GetByDonHangAsync(int donHangId)
        {
            return await _context.LichSuTichDiems
                .Where(l => l.MaDonHang == donHangId)
                .ToListAsync();
        }
    }
}
