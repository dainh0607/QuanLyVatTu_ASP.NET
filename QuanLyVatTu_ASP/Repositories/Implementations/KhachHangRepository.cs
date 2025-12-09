using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using BCrypt.Net;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class KhachHangRepository : IKhachHangRepository
    {
        private readonly ApplicationDbContext _context;

        public KhachHangRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<KhachHang> GetByIdAsync(int id)
        {
            return await _context.KhachHangs.FindAsync(id);
        }

        public async Task<KhachHang> GetByEmailAsync(string email)
        {
            return await _context.KhachHangs.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<KhachHang> GetByTaiKhoanAsync(string taiKhoan)
        {
            return await _context.KhachHangs.FirstOrDefaultAsync(u => u.TaiKhoan == taiKhoan);
        }

        public async Task<KhachHang> GetByMaHienThiAsync(string maHienThi)
        {
            return await _context.KhachHangs.FirstOrDefaultAsync(u => u.MaHienThi == maHienThi);
        }

        public KhachHang GetByLogin(string loginInput, string password)
        {
            var user = _context.KhachHangs
                .FirstOrDefault(x => x.Email == loginInput || x.TaiKhoan == loginInput);

            if (user == null) return null;

            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.MatKhau);
            }
            catch
            {
                return null;
            }

            if (isPasswordValid)
            {
                return user;
            }

            return null;
        }

        public void Add(KhachHang khachHang)
        {
            _context.KhachHangs.Add(khachHang);
        }

        public async Task<KhachHang> UpdateAsync(KhachHang khachHang)
        {
            var existingUser = await _context.KhachHangs.FindAsync(khachHang.MaHienThi);

            if (existingUser != null)
            {
                existingUser.HoTen = khachHang.HoTen;
                existingUser.SoDienThoai = khachHang.SoDienThoai;
                existingUser.DiaChi = khachHang.DiaChi;

                _context.KhachHangs.Update(existingUser);
            }
            return existingUser;
        }
    }
}