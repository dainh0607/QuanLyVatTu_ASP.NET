using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class VoucherRepository : GenericRepository<Voucher>, IVoucherRepository
    {
        public VoucherRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Voucher?> GetByCodeAsync(string code)
        {
            return await _context.Vouchers
                .FirstOrDefaultAsync(v => v.MaVoucher == code);
        }

        public async Task<IEnumerable<Voucher>> GetActiveVouchersAsync()
        {
            var now = DateTime.Now;
            return await _context.Vouchers
                .Where(v => v.TrangThaiGoc == "ACTIVE"
                         && v.ThoiGianBatDau <= now
                         && v.ThoiGianKetThuc > now
                         && v.SoLuongDaDung < v.TongSoLuong)
                .OrderByDescending(v => v.NgayTao)
                .ToListAsync();
        }
    }
}
