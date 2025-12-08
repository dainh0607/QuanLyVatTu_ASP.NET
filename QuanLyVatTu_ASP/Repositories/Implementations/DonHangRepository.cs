using Microsoft.Identity.Client;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class DonHangRepository : GenericRepository<DonHang>, IDonHangRepository
    {
        public DonHangRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<KhachHang> GetDonHangByKhachHangAsync(int khachHangId)
        {
            var donHang = await _dbSet
                .Include(dh => dh.KhachHang) // Ensure 'Microsoft.EntityFrameworkCore' is referenced
                .FirstOrDefaultAsync(dh => dh.KhachHangId == khachHangId);

            return donHang?.KhachHang ?? throw new InvalidOperationException("Khách hàng không tồn tại.");
        }
    }
}

