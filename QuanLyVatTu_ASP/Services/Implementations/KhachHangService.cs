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
                keyword = keyword.ToLower();
                query = query.Where(x =>
                    x.MaHienThi.ToLower().Contains(keyword) ||
                    x.HoTen.ToLower().Contains(keyword) ||
                    x.Email.ToLower().Contains(keyword) ||
                    (x.SoDienThoai != null && x.SoDienThoai.ToLower().Contains(keyword)));
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
                TaiKhoan = kh.TaiKhoan,
                AnhDaiDien = kh.AnhDaiDien
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

            string maHienThiAuto = await GetNextMaHienThiAsync();

            var kh = new KhachHang
            {
                MaHienThi = maHienThiAuto,
                HoTen = model.HoTen,
                Email = model.Email ?? "",
                SoDienThoai = model.SoDienThoai ?? "",
                DiaChi = model.DiaChi ?? "",
                TaiKhoan = model.TaiKhoan,
                AnhDaiDien = model.AnhDaiDien,

                // MatKhau = BCryptNet.HashPassword(model.MatKhau),
                MatKhau = model.MatKhau ?? "",

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
            kh.Email = model.Email ?? "";
            kh.SoDienThoai = model.SoDienThoai ?? "";
            kh.DiaChi = model.DiaChi ?? "";
            kh.TaiKhoan = model.TaiKhoan;
            
            // Cập nhật ảnh nếu có
            if (!string.IsNullOrEmpty(model.AnhDaiDien))
            {
                kh.AnhDaiDien = model.AnhDaiDien;
            }

            if (!string.IsNullOrEmpty(model.MatKhau))
            {
                // kh.MatKhau = BCryptNet.HashPassword(model.MatKhau);
                kh.MatKhau = model.MatKhau ?? "";
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

        /// <summary>
        /// Sinh mã hiển thị tiếp theo - tìm mã bị thiếu trong dãy KH001, KH002...
        /// </summary>
        public async Task<string> GetNextMaHienThiAsync()
        {
            var existingCodes = await _context.KhachHangs
                .Select(x => x.MaHienThi)
                .Where(x => x.StartsWith("KH"))
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => {
                    if (x.Length > 2 && int.TryParse(x.Substring(2), out int n))
                        return n;
                    return 0;
                })
                .Where(x => x > 0)
                .ToHashSet();

            int nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return $"KH{nextNumber:D3}";
        }

        public async Task<KhachHang?> GetByIdAsync(int id)
        {
            return await _context.KhachHangs.FindAsync(id);
        }
    }
}