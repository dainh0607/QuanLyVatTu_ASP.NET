using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhaCungCap;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class NhaCungCapService : INhaCungCapService
    {
        private readonly AppDbContext _context;

        public NhaCungCapService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NhaCungCapIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize)
        {
            if (page < 1) page = 1;

            var query = _context.NhaCungCaps.Include(x => x.VatTus).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(x =>
                   
                    x.TenNhaCungCap.ToLower().Contains(keyword) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.SoDienThoai != null && x.SoDienThoai.ToLower().Contains(keyword)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NhaCungCapIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi,
                    TenNhaCungCap = x.TenNhaCungCap,
                    Email = x.Email,
                    SoDienThoai = x.SoDienThoai,
                    DiaChi = x.DiaChi,
                    SoLuongVatTuCungCap = x.VatTus.Count,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            return new NhaCungCapIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<NhaCungCapCreateEditViewModel?> GetByIdForEditAsync(int id)
        {
            var ncc = await _context.NhaCungCaps.FindAsync(id);
            if (ncc == null) return null;

            return new NhaCungCapCreateEditViewModel
            {
                Id = ncc.ID,
                TenNhaCungCap = ncc.TenNhaCungCap,
                Email = ncc.Email,
                SoDienThoai = ncc.SoDienThoai,
                DiaChi = ncc.DiaChi
            };
        }

        public async Task<string?> CreateAsync(NhaCungCapCreateEditViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Email) && await _context.NhaCungCaps.AnyAsync(x => x.Email == model.Email))
            {
                return "Email này đã tồn tại trong hệ thống.";
            }

            if (await _context.NhaCungCaps.AnyAsync(x => x.TenNhaCungCap == model.TenNhaCungCap))
            {
                return "Tên nhà cung cấp này đã tồn tại.";
            }

            var ncc = new NhaCungCap
            {
                MaHienThi = "NCC" + new Random().Next(1000, 9999),
                TenNhaCungCap = model.TenNhaCungCap,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                DiaChi = model.DiaChi,
                NgayTao = DateTime.Now
            };

            _context.NhaCungCaps.Add(ncc);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> UpdateAsync(int id, NhaCungCapCreateEditViewModel model)
        {
            var ncc = await _context.NhaCungCaps.FindAsync(id);
            if (ncc == null) return "Nhà cung cấp không tồn tại";

            if (!string.IsNullOrEmpty(model.Email) &&
                await _context.NhaCungCaps.AnyAsync(x => x.Email == model.Email && x.ID != id))
            {
                return "Email này đã được sử dụng bởi nhà cung cấp khác.";
            }

            if (await _context.NhaCungCaps.AnyAsync(x => x.TenNhaCungCap == model.TenNhaCungCap && x.ID != id))
            {
                return "Tên nhà cung cấp này đã tồn tại.";
            }

            ncc.TenNhaCungCap = model.TenNhaCungCap;
            ncc.Email = model.Email;
            ncc.SoDienThoai = model.SoDienThoai;
            ncc.DiaChi = model.DiaChi;

            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var ncc = await _context.NhaCungCaps
                .Include(x => x.VatTus)
                .FirstOrDefaultAsync(x => x.ID == id);

            if (ncc == null) return "Nhà cung cấp không tồn tại";

            if (ncc.VatTus != null && ncc.VatTus.Any())
            {
                return $"Không thể xóa! Nhà cung cấp này đang cung cấp {ncc.VatTus.Count} vật tư.";
            }

            _context.NhaCungCaps.Remove(ncc);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<List<NhaCungCap>> GetLookupAsync()
        {
            return await _context.NhaCungCaps.OrderBy(x => x.TenNhaCungCap).ToListAsync();
        }
    }
}