using QuanLyVatTu_ASP.Areas.Admin.Models;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface IVoucherRepository : IGenericRepository<Voucher>
    {
        Task<Voucher?> GetByCodeAsync(string code);
        Task<IEnumerable<Voucher>> GetActiveVouchersAsync();
    }
}
