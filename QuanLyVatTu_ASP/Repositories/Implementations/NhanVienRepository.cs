using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using BCrypt.Net;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class NhanVienRepository : GenericRepository<NhanVien>, INhanVienRepository
    {
        // Khai báo context riêng để query dễ dàng
        private readonly ApplicationDbContext _context;

        public NhanVienRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // --- HÀM ĐÃ SỬA ĐỔI ĐỂ DÙNG BCRYPT ---
        public NhanVien GetByLogin(string email, string password)
        {
            // 1. Tìm nhân viên theo Email hoặc Tài khoản
            var nhanVien = _context.NhanViens
                .FirstOrDefault(x => x.Email == email || x.TaiKhoan == email);

            if (nhanVien == null) return null;

            // 2. Verify mật khẩu
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