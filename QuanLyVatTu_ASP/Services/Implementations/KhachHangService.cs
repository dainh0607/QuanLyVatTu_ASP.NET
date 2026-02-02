using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.KhachHangViewModels;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class KhachHangService : IKhachHangService
    {
        private readonly AppDbContext _context;

        public KhachHangService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<KhachHangIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize)
        {
            if (page < 1) page = 1;
            var query = _context.KhachHangs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.MaHienThi.Contains(keyword) ||
                    x.HoTen.Contains(keyword) ||
                    x.Email.Contains(keyword) ||
                    (x.SoDienThoai != null && x.SoDienThoai.Contains(keyword)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new KhachHangIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi,
                    HoTen = x.HoTen,
                    Email = x.Email,
                    SoDienThoai = x.SoDienThoai,
                    DiaChi = x.DiaChi,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            return new KhachHangIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<KhachHangCreateEditViewModel?> GetByIdForEditAsync(int id)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return null;

            return new KhachHangCreateEditViewModel
            {
                Id = kh.ID,
                HoTen = kh.HoTen,
                Email = kh.Email,
                SoDienThoai = kh.SoDienThoai,
                DiaChi = kh.DiaChi,
                TaiKhoan = kh.TaiKhoan
            };
        }

        public async Task<string?> CreateAsync(KhachHangCreateEditViewModel model)
        {
            if (await _context.KhachHangs.AnyAsync(x => x.Email == model.Email))
                return "Email này đã tồn tại.";

            if (await _context.KhachHangs.AnyAsync(x => x.TaiKhoan == model.TaiKhoan))
                return "Tài khoản này đã tồn tại.";

            if (!string.IsNullOrEmpty(model.SoDienThoai) && await _context.KhachHangs.AnyAsync(x => x.SoDienThoai == model.SoDienThoai))
                return "Số điện thoại này đã tồn tại.";

            string randomSuffix = new Random().Next(1000, 9999).ToString();
            string maHienThiAuto = "KH" + randomSuffix;

            var kh = new KhachHang
            {
                MaHienThi = maHienThiAuto,
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                DiaChi = model.DiaChi,
                TaiKhoan = model.TaiKhoan,

                // MatKhau = BCryptNet.HashPassword(model.MatKhau),
                MatKhau = model.MatKhau,

                DangNhapGoogle = false,
                NgayTao = DateTime.Now
            };

            _context.KhachHangs.Add(kh);
            await _context.SaveChangesAsync();

            return null;
        }

        public async Task<string?> UpdateAsync(int id, KhachHangCreateEditViewModel model)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return "Khách hàng không tồn tại.";

            if (await _context.KhachHangs.AnyAsync(x => x.Email == model.Email && x.ID != id))
                return "Email đã được sử dụng bởi khách hàng khác.";

            if (await _context.KhachHangs.AnyAsync(x => x.TaiKhoan == model.TaiKhoan && x.ID != id))
                return "Tài khoản đã được sử dụng bởi khách hàng khác.";

            if (!string.IsNullOrEmpty(model.SoDienThoai) && await _context.KhachHangs.AnyAsync(x => x.SoDienThoai == model.SoDienThoai && x.ID != id))
                return "SĐT đã được sử dụng bởi khách hàng khác.";

            kh.HoTen = model.HoTen;
            kh.Email = model.Email;
            kh.SoDienThoai = model.SoDienThoai;
            kh.DiaChi = model.DiaChi;
            kh.TaiKhoan = model.TaiKhoan;

            if (!string.IsNullOrEmpty(model.MatKhau))
            {
                // kh.MatKhau = BCryptNet.HashPassword(model.MatKhau);
                kh.MatKhau = model.MatKhau;
            }

            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return "Khách hàng không tồn tại";

            bool hasDonHang = await _context.DonHang.AnyAsync(d => d.KhachHangId == id);
            if (hasDonHang) return "Không thể xóa! Khách hàng này đã có đơn hàng.";

            _context.KhachHangs.Remove(kh);
            await _context.SaveChangesAsync();
            return null;
        }
    }
}