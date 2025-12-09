using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhanVien;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class NhanVienService : INhanVienService
    {
        private readonly ApplicationDbContext _context;

        public NhanVienService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<NhanVienIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize)
        {
            if (page < 1) page = 1;
            var query = _context.NhanViens.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.MaHienThi.Contains(keyword) ||
                    x.HoTen.Contains(keyword) ||
                    x.CCCD.Contains(keyword) ||
                    x.SoDienThoai.Contains(keyword) ||
                    x.VaiTro.Contains(keyword));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NhanVienIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi,
                    HoTen = x.HoTen,
                    NgaySinh = x.NgaySinh,
                    CCCD = x.CCCD,
                    SoDienThoai = x.SoDienThoai,
                    Email = x.Email,
                    VaiTro = x.VaiTro,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            return new NhanVienIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<NhanVienCreateEditViewModel> GetByIdForEditAsync(int id)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null) return null;

            return new NhanVienCreateEditViewModel
            {
                Id = nv.ID,
                HoTen = nv.HoTen,
                NgaySinh = nv.NgaySinh,
                CCCD = nv.CCCD,
                SoDienThoai = nv.SoDienThoai,
                Email = nv.Email,
                TaiKhoan = nv.TaiKhoan,
                VaiTro = nv.VaiTro
            };
        }

        public async Task<string> CreateAsync(NhanVienCreateEditViewModel model)
        {
            if (await _context.NhanViens.AnyAsync(x => x.CCCD == model.CCCD))
                return "CCCD này đã tồn tại.";

            if (await _context.NhanViens.AnyAsync(x => x.SoDienThoai == model.SoDienThoai))
                return "Số điện thoại này đã tồn tại.";

            if (!string.IsNullOrEmpty(model.Email) && await _context.NhanViens.AnyAsync(x => x.Email == model.Email))
                return "Email này đã tồn tại.";

            if (await _context.NhanViens.AnyAsync(x => x.TaiKhoan == model.TaiKhoan))
                return "Tài khoản đăng nhập đã tồn tại.";

            var nhanVien = new NhanVien
            {
                HoTen = model.HoTen,
                NgaySinh = model.NgaySinh,
                CCCD = model.CCCD,
                SoDienThoai = model.SoDienThoai,
                Email = model.Email,
                TaiKhoan = model.TaiKhoan,
                VaiTro = model.VaiTro,
                NgayTao = DateTime.Now,

                MatKhau = BCryptNet.HashPassword(model.MatKhau)
            };

            _context.NhanViens.Add(nhanVien);
            await _context.SaveChangesAsync();

            return null;
        }

        public async Task<string> UpdateAsync(int id, NhanVienCreateEditViewModel model)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return "Nhân viên không tồn tại.";

            if (await _context.NhanViens.AnyAsync(x => x.CCCD == model.CCCD && x.ID != id))
                return "CCCD đã được sử dụng bởi nhân viên khác.";

            if (await _context.NhanViens.AnyAsync(x => x.SoDienThoai == model.SoDienThoai && x.ID != id))
                return "SĐT đã được sử dụng bởi nhân viên khác.";

            if (!string.IsNullOrEmpty(model.Email) && await _context.NhanViens.AnyAsync(x => x.Email == model.Email && x.ID != id))
                return "Email đã được sử dụng bởi nhân viên khác.";

            if (await _context.NhanViens.AnyAsync(x => x.TaiKhoan == model.TaiKhoan && x.ID != id))
                return "Tài khoản này đã tồn tại.";

            nhanVien.HoTen = model.HoTen;
            nhanVien.NgaySinh = model.NgaySinh;
            nhanVien.CCCD = model.CCCD;
            nhanVien.SoDienThoai = model.SoDienThoai;
            nhanVien.Email = model.Email;
            nhanVien.TaiKhoan = model.TaiKhoan;
            nhanVien.VaiTro = model.VaiTro;

            if (!string.IsNullOrEmpty(model.MatKhau))
            {
                nhanVien.MatKhau = BCryptNet.HashPassword(model.MatKhau);
            }

            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string> DeleteAsync(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return "Nhân viên không tồn tại";

            bool hasDonHang = await _context.DonHang.AnyAsync(d => d.NhanVienId == id);
            if (hasDonHang) return "Không thể xóa! Nhân viên này đã có lịch sử lập đơn hàng.";

            _context.NhanViens.Remove(nhanVien);
            await _context.SaveChangesAsync();
            return null;
        }
    }
}
