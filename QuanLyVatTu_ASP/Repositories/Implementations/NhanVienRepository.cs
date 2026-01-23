using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using BCrypt.Net;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class NhanVienRepository : GenericRepository<NhanVien>, INhanVienRepository
    {
        private readonly AppDbContext _context;

        public NhanVienRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public NhanVien GetByLogin(string email, string password)
        {
            var nhanVien = _context.NhanViens
                .FirstOrDefault(x => x.Email == email || x.TaiKhoan == email);

            if (nhanVien == null) return null;
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

            return null;
        }
    }
}