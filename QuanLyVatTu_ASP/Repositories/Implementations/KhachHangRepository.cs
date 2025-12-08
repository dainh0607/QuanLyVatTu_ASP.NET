// File: Repositories/Implementations/KhachHangRepository.cs
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Implementations
{
    public class KhachHangRepository : GenericRepository<KhachHang>, IKhachHangRepository
    {
        private readonly ApplicationDbContext _context;

        public KhachHangRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<KhachHang> GetByEmailAsync(string email)
        {
            return await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.Email == email);
        }

        public async Task<KhachHang> GetByTaiKhoanAsync(string taiKhoan)
        {
            return await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.TaiKhoan == taiKhoan);
        }

        public async Task<KhachHang> GetByMaHienThiAsync(string maHienThi)
        {
            return await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.MaHienThi == maHienThi);
        }
        public async Task<KhachHang> GetByIdAsync(int id)
        {
            return await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.ID == id);
        }
        public async Task<KhachHang> UpdateAsync(KhachHang khachHang)
        {
            _context.KhachHangs.Update(khachHang);
            await _context.SaveChangesAsync();
            return khachHang;
        }
    }
}