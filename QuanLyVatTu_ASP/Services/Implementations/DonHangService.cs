using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class DonHangService : IDonHangService
    {
        private readonly ApplicationDbContext _context;

        public DonHangService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DonHangIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize)
        {
            if (page < 1) page = 1;

            // Include bảng KhachHang và NhanVien để lấy tên hiển thị
            var query = _context.DonHang
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.MaHienThi.Contains(keyword) ||
                    (x.GhiChu != null && x.GhiChu.Contains(keyword)) ||
                    x.KhachHang.HoTen.Contains(keyword));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DonHangIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi ?? "DH" + x.ID.ToString("0000"),
                    TenKhachHang = x.KhachHang.HoTen,
                    TenNhanVien = x.NhanVien != null ? x.NhanVien.HoTen : "",
                    NgayDat = x.NgayDat,
                    TongTien = x.TongTien,
                    SoTienDatCoc = x.SoTienDatCoc ?? 0,
                    PhuongThucDatCoc = x.PhuongThucDatCoc,
                    NgayDatCoc = x.NgayDatCoc,
                    TrangThai = string.IsNullOrWhiteSpace(x.TrangThai) ? "Chờ xác nhận" : x.TrangThai,
                    GhiChu = x.GhiChu,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            return new DonHangIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<DonHangCreateEditViewModel> GetByIdForEditAsync(int id)
        {
            var entity = await _context.DonHang.FindAsync(id);
            if (entity == null) return null;

            return new DonHangCreateEditViewModel
            {
                Id = entity.ID,
                MaHienThi = entity.MaHienThi,
                KhachHangId = entity.KhachHangId ?? 0,
                NhanVienId = entity.NhanVienId,
                NgayDat = entity.NgayDat,
                TongTien = entity.TongTien,
                SoTienDatCoc = entity.SoTienDatCoc,
                PhuongThucDatCoc = entity.PhuongThucDatCoc,
                NgayDatCoc = entity.NgayDatCoc,
                TrangThai = entity.TrangThai ?? "Chờ xác nhận",
                GhiChu = entity.GhiChu
            };
        }

        public async Task CreateAsync(DonHangCreateEditViewModel model)
        {
            var entity = new DonHang
            {
                KhachHangId = model.KhachHangId,
                NhanVienId = model.NhanVienId ?? 0,
                NgayDat = model.NgayDat,
                TongTien = model.TongTien,
                SoTienDatCoc = model.SoTienDatCoc,
                PhuongThucDatCoc = model.PhuongThucDatCoc,
                NgayDatCoc = model.NgayDatCoc,
                TrangThai = model.TrangThai ?? "Chờ xác nhận",
                GhiChu = model.GhiChu,
                NgayTao = DateTime.Now
            };

            _context.DonHang.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(int id, DonHangCreateEditViewModel model)
        {
            var entity = await _context.DonHang.FindAsync(id);
            if (entity == null) return false;

            entity.KhachHangId = model.KhachHangId;
            entity.NhanVienId = model.NhanVienId ?? 0;
            entity.NgayDat = model.NgayDat;
            entity.SoTienDatCoc = model.SoTienDatCoc;
            entity.PhuongThucDatCoc = model.PhuongThucDatCoc;
            entity.NgayDatCoc = model.NgayDatCoc;
            entity.TrangThai = model.TrangThai ?? "Chờ xác nhận";
            entity.GhiChu = model.GhiChu;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.DonHang.FindAsync(id);
            if (entity != null)
            {
                _context.DonHang.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<KhachHang>> GetKhachHangLookupAsync()
        {
            return await _context.KhachHangs
                .Select(k => new KhachHang { ID = k.ID, HoTen = k.HoTen })
                .ToListAsync();
        }

        public async Task<List<NhanVien>> GetNhanVienLookupAsync()
        {
            return await _context.NhanViens
                .Select(n => new NhanVien { ID = n.ID, HoTen = n.HoTen })
                .ToListAsync();
        }
    }
}