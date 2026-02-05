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
        private readonly AppDbContext _context;

        public NhanVienService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NhanVienIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize)
        {
            if (page < 1) page = 1;
            var query = _context.NhanViens.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(x =>
                    x.MaHienThi.ToLower().Contains(keyword) ||
                    x.HoTen.ToLower().Contains(keyword) ||
                    x.CCCD.ToLower().Contains(keyword) ||
                    x.SoDienThoai.ToLower().Contains(keyword) ||
                    x.VaiTro.ToLower().Contains(keyword));
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

        public async Task<NhanVienCreateEditViewModel?> GetByIdForEditAsync(int id)
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
                VaiTro = nv.VaiTro,
                AnhDaiDien = nv.AnhDaiDien
            };
        }

        public async Task<string?> CreateAsync(NhanVienCreateEditViewModel model)
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
                MaHienThi = await GetNextMaHienThiAsync(),
                HoTen = model.HoTen,
                NgaySinh = model.NgaySinh,
                CCCD = model.CCCD,
                SoDienThoai = model.SoDienThoai,
                Email = model.Email,
                TaiKhoan = model.TaiKhoan,
                VaiTro = model.VaiTro,
                AnhDaiDien = model.AnhDaiDien,
                NgayTao = DateTime.Now,

                MatKhau = BCryptNet.HashPassword(model.MatKhau)
                //MatKhau = model.MatKhau ?? ""
            };

            _context.NhanViens.Add(nhanVien);
            await _context.SaveChangesAsync();

            return null;
        }

        public async Task<string?> UpdateAsync(int id, NhanVienCreateEditViewModel model)
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
            
            // Cập nhật ảnh nếu có
            if (!string.IsNullOrEmpty(model.AnhDaiDien))
            {
                nhanVien.AnhDaiDien = model.AnhDaiDien;
            }

            if (!string.IsNullOrEmpty(model.MatKhau))
            {
                nhanVien.MatKhau = BCryptNet.HashPassword(model.MatKhau);
                //nhanVien.MatKhau = model.MatKhau ?? "";
            }

            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return "Nhân viên không tồn tại";

            bool hasDonHang = await _context.DonHang.AnyAsync(d => d.NhanVienId == id);
            if (hasDonHang) return "Không thể xóa! Nhân viên này đã có lịch sử lập đơn hàng.";

            _context.NhanViens.Remove(nhanVien);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<UserProfileViewModel?> GetByEmailAsync(string email)
        {
            var user = await _context.NhanViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null) return null;

            return new UserProfileViewModel
            {
                Id = user.ID,
                HoTen = user.HoTen,
                VaiTro = user.VaiTro,
                // Lấy ảnh từ DB, nếu không có thì để trống
                Avatar = user.AnhDaiDien ?? string.Empty
            };
        }

        /// <summary>
        /// Sinh mã hiển thị tiếp theo - tìm mã bị thiếu trong dãy NV001, NV002...
        /// </summary>
        public async Task<string> GetNextMaHienThiAsync()
        {
            var existingCodes = await _context.NhanViens
                .Select(x => x.MaHienThi)
                .Where(x => x.StartsWith("NV"))
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

            return $"NV{nextNumber:D3}";
        }

        public async Task<NhanVien?> GetByIdAsync(int id)
        {
            return await _context.NhanViens.FindAsync(id);
        }
    }
}
