using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using BCrypt.Net;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class NhanVienRepository : GenericRepository<NhanVien>, INhanVienRepository
    {
        public NhanVienRepository(AppDbContext context) : base(context)
        {
        }

        public NhanVien? GetByLogin(string email, string password)
        {
            var nhanVien = _context.NhanViens
                .FirstOrDefault(x => x.Email == email || x.TaiKhoan == email);

            if (nhanVien == null) return null;
            /* COMMENT BCrypt THEO YÊU CẦU
            try
            {
                if (BCrypt.Net.BCrypt.Verify(password, nhanVien.MatKhau))
                {
                    return nhanVien;
                }
            }
            catch
            {
                return null;
            }
            */

            return null;
        }
    }
}