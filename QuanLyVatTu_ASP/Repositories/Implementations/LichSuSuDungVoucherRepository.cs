using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class LichSuSuDungVoucherRepository : GenericRepository<LichSuSuDungVoucher>, ILichSuSuDungVoucherRepository
    {
        public LichSuSuDungVoucherRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Đếm số lượt APPLIED + BURNED (không đếm REFUNDED) để kiểm tra usage_limit_per_user
        /// </summary>
        public async Task<int> CountUsageAsync(int khachHangId, int voucherId)
        {
            return await _context.LichSuSuDungVouchers
                .CountAsync(l => l.MaKhachHang == khachHangId
                              && l.MaVoucherGoc == voucherId
                              && (l.TrangThaiSuDung == "APPLIED" || l.TrangThaiSuDung == "BURNED"));
        }

        /// <summary>
        /// Lấy lịch sử sử dụng voucher theo đơn hàng (1 đơn chỉ có 1 voucher)
        /// </summary>
        public async Task<LichSuSuDungVoucher?> GetByDonHangAsync(int donHangId)
        {
            return await _context.LichSuSuDungVouchers
                .Include(l => l.VoucherGoc)
                .FirstOrDefaultAsync(l => l.MaDonHang == donHangId);
        }
    }
}
