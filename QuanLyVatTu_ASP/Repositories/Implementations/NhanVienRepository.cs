using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class NhanVienRepository : GenericRepository<NhanVien>, INhanVienRepository
    {
        public NhanVienRepository(ApplicationDbContext context) : base(context)
        {
        }
        public NhanVien GetByLogin(string email, string password)
        {
            return _context.NhanViens
                .FirstOrDefault(x => (x.Email == email || x.TaiKhoan == email) && x.MatKhau == password);
        }
    }
}
