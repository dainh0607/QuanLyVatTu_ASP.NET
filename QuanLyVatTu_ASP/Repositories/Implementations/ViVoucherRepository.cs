using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class ViVoucherRepository : GenericRepository<ViVoucherKhachHang>, IViVoucherRepository
    {
        public ViVoucherRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ViVoucherKhachHang>> GetByKhachHangAsync(int khachHangId)
        {
            return await _context.ViVoucherKhachHangs
                .Include(v => v.VoucherGoc)
                .Where(v => v.MaKhachHang == khachHangId)
                .OrderByDescending(v => v.ThoiGianLuuMa)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int khachHangId, int voucherId)
        {
            return await _context.ViVoucherKhachHangs
                .AnyAsync(v => v.MaKhachHang == khachHangId && v.MaVoucherGoc == voucherId);
        }

        public async Task<IEnumerable<ViVoucherKhachHang>> GetAvailableAsync(int khachHangId)
        {
            var now = DateTime.Now;
            return await _context.ViVoucherKhachHangs
                .Include(v => v.VoucherGoc)
                .Where(v => v.MaKhachHang == khachHangId
                         && v.TrangThaiTrongVi == "AVAILABLE"
                         && v.VoucherGoc != null
                         && v.VoucherGoc.ThoiGianKetThuc > now
                         && v.VoucherGoc.TrangThaiGoc == "ACTIVE")
                .OrderByDescending(v => v.ThoiGianLuuMa)
                .ToListAsync();
        }

        public async Task<ViVoucherKhachHang?> GetByKhachHangAndVoucherAsync(int khachHangId, int voucherId)
        {
            return await _context.ViVoucherKhachHangs
                .Include(v => v.VoucherGoc)
                .FirstOrDefaultAsync(v => v.MaKhachHang == khachHangId && v.MaVoucherGoc == voucherId);
        }
    }
}
