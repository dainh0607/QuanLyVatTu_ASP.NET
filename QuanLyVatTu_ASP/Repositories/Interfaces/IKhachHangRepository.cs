using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Repositories.Interfaces
{
    public interface IKhachHangRepository
    {
        Task<KhachHang> GetByEmailAsync(string email);
        Task<KhachHang> GetByTaiKhoanAsync(string taiKhoan);
        Task<KhachHang> GetByMaHienThiAsync(string maHienThi);
        Task<KhachHang> GetByIdAsync(int id);
        Task<KhachHang> UpdateAsync(KhachHang khachHang);
        KhachHang GetByLogin(string email, string password);
        void Add(KhachHang khachHang);
    }
}