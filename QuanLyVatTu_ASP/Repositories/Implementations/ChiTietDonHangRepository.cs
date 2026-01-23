using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class ChiTietDonHangRepository : GenericRepository<ChiTietDonHang>, IChiTietDonHangRepository
    {
        public ChiTietDonHangRepository(AppDbContext context) : base(context)
        {
        }
    }
}
