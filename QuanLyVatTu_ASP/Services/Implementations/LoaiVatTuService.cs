using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.Admin.LoaiVatTu;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.LoaiVatTu;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class LoaiVatTuService : ILoaiVatTuService
    {
        private readonly AppDbContext _context;

        public LoaiVatTuService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LoaiVatTuIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize)
        {
            if (page < 1) page = 1;

            var query = _context.LoaiVatTus.Include(x => x.VatTus).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(x =>
                    
                    x.TenLoaiVatTu.ToLower().Contains(keyword) ||
                    (x.MoTa != null && x.MoTa.ToLower().Contains(keyword)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LoaiVatTuIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi,
                    TenLoaiVatTu = x.TenLoaiVatTu,
                    MoTa = x.MoTa,
                    SoLuongVatTu = x.VatTus.Count,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            return new LoaiVatTuIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<LoaiVatTuCreateEditViewModel> GetByIdForEditAsync(int id)
        {
            var loai = await _context.LoaiVatTus.FindAsync(id);
            if (loai == null) return null;

            return new LoaiVatTuCreateEditViewModel
            {
                Id = loai.ID,
                TenLoaiVatTu = loai.TenLoaiVatTu,
                MoTa = loai.MoTa
            };
        }

        public async Task<string> CreateAsync(LoaiVatTuCreateEditViewModel model)
        {
            if (await _context.LoaiVatTus.AnyAsync(x => x.TenLoaiVatTu == model.TenLoaiVatTu))
            {
                return "Tên loại vật tư này đã tồn tại";
            }

            var loai = new LoaiVatTu
            {
                MaHienThi = "LVT" + new Random().Next(1000, 9999),
                TenLoaiVatTu = model.TenLoaiVatTu,
                MoTa = model.MoTa,
                NgayTao = DateTime.Now
            };

            _context.LoaiVatTus.Add(loai);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string> UpdateAsync(int id, LoaiVatTuCreateEditViewModel model)
        {
            var loai = await _context.LoaiVatTus.FindAsync(id);
            if (loai == null) return "Loại vật tư không tồn tại";

            if (await _context.LoaiVatTus.AnyAsync(x => x.TenLoaiVatTu == model.TenLoaiVatTu && x.ID != id))
            {
                return "Tên loại vật tư đã được sử dụng";
            }

            loai.TenLoaiVatTu = model.TenLoaiVatTu;
            loai.MoTa = model.MoTa;

            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string> DeleteAsync(int id)
        {
            var loai = await _context.LoaiVatTus
                .Include(x => x.VatTus)
                .FirstOrDefaultAsync(x => x.ID == id);

            if (loai == null) return "Loại vật tư không tồn tại";

            if (loai.VatTus != null && loai.VatTus.Any())
            {
                return $"Không thể xóa! Có {loai.VatTus.Count} vật tư đang thuộc loại này.";
            }

            _context.LoaiVatTus.Remove(loai);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<List<LoaiVatTu>> GetLookupAsync()
        {
            return await _context.LoaiVatTus.OrderBy(x => x.TenLoaiVatTu).ToListAsync();
        }
    }
}