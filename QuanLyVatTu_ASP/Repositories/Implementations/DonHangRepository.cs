// File: Repositories/Implementations/DonHangRepository.cs
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class DonHangRepository : GenericRepository<DonHang>, IDonHangRepository
    {
        public DonHangRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<DonHang>> GetDonHangByKhachHangAsync(int khachHangId)
        {
            return await _context.DonHang
                .Where(x => x.KhachHangId == khachHangId)
                .Include(x => x.ChiTietDonHangs)
                .ThenInclude(ct => ct.VatTu)
                .OrderByDescending(x => x.NgayDat)
                .ToListAsync();
        }
    }
}