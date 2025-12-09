using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.VatTu;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class VatTuService : IVatTuService
    {
        private readonly ApplicationDbContext _context;

        public VatTuService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VatTuIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize)
        {
            if (page < 1) page = 1;

            var query = _context.VatTus
                .Include(v => v.LoaiVatTu)
                .Include(v => v.NhaCungCap)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                // BÂY GIỜ ĐÃ CÓ THỂ TÌM KIẾM THEO MaHienThi TRONG DB
                query = query.Where(x =>
                    x.MaHienThi.Contains(keyword) ||
                    x.TenVatTu.Contains(keyword) ||
                    x.DonViTinh.Contains(keyword) ||
                    x.LoaiVatTu.TenLoaiVatTu.Contains(keyword) ||
                    x.NhaCungCap.TenNhaCungCap.Contains(keyword));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.NgayTao) // SQL có cột NgayTao, sắp xếp vô tư
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new VatTuIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi, // Lấy trực tiếp từ DB, không cần $"VT..."
                    TenVatTu = x.TenVatTu,
                    DonViTinh = x.DonViTinh,
                    GiaNhap = x.GiaNhap,
                    GiaBan = x.GiaBan,
                    SoLuongTon = x.SoLuongTon,
                    TenLoaiVatTu = x.LoaiVatTu != null ? x.LoaiVatTu.TenLoaiVatTu : "",
                    TenNhaCungCap = x.NhaCungCap != null ? x.NhaCungCap.TenNhaCungCap : "",
                    NgayTao = x.NgayTao ?? DateTime.MinValue
                })
                .ToListAsync();

            return new VatTuIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<VatTuCreateEditViewModel> GetByIdForEditAsync(int id)
        {
            var vt = await _context.VatTus.FindAsync(id);
            if (vt == null) return null;

            return new VatTuCreateEditViewModel
            {
                Id = vt.ID,
                TenVatTu = vt.TenVatTu,
                DonViTinh = vt.DonViTinh,
                GiaNhap = vt.GiaNhap,
                GiaBan = vt.GiaBan,
                SoLuongTon = vt.SoLuongTon,

                // Cập nhật lại tên biến theo đúng SQL
                MaLoaiVatTu = vt.MaLoaiVatTu,
                MaNhaCungCap = vt.MaNhaCungCap
            };
        }

        public async Task<string> CreateAsync(VatTuCreateEditViewModel model)
        {
            if (await _context.VatTus.AnyAsync(x => x.TenVatTu == model.TenVatTu))
            {
                return "Tên vật tư này đã tồn tại trong kho.";
            }

            var vt = new VatTu
            {
                // KHÔNG GÁN MaHienThi Ở ĐÂY (DB tự sinh)
                TenVatTu = model.TenVatTu,
                DonViTinh = model.DonViTinh,
                GiaNhap = model.GiaNhap,
                GiaBan = model.GiaBan,
                SoLuongTon = model.SoLuongTon,

                MaLoaiVatTu = model.MaLoaiVatTu,
                MaNhaCungCap = model.MaNhaCungCap,

                NgayTao = DateTime.Now
            };

            _context.VatTus.Add(vt);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string> UpdateAsync(int id, VatTuCreateEditViewModel model)
        {
            var vt = await _context.VatTus.FindAsync(id);
            if (vt == null) return "Vật tư không tồn tại";

            if (await _context.VatTus.AnyAsync(x => x.TenVatTu == model.TenVatTu && x.ID != id))
            {
                return "Tên vật tư đã được sử dụng.";
            }

            // KHÔNG UPDATE MaHienThi
            vt.TenVatTu = model.TenVatTu;
            vt.DonViTinh = model.DonViTinh;
            vt.GiaNhap = model.GiaNhap;
            vt.GiaBan = model.GiaBan;
            vt.SoLuongTon = model.SoLuongTon;

            vt.MaLoaiVatTu = model.MaLoaiVatTu;
            vt.MaNhaCungCap = model.MaNhaCungCap;

            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string> DeleteAsync(int id)
        {
            var vt = await _context.VatTus.FindAsync(id);
            if (vt == null) return "Vật tư không tồn tại";

            bool inDonHang = await _context.ChiTietDonHangs.AnyAsync(ct => ct.MaVatTu == id);
            bool inHoaDon = await _context.ChiTietHoaDons.AnyAsync(ct => ct.MaVatTu == id);

            if (inDonHang || inHoaDon)
            {
                return "Không thể xóa! Vật tư này đã phát sinh giao dịch.";
            }

            _context.VatTus.Remove(vt);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<(List<LoaiVatTu> LoaiList, List<NhaCungCap> NccList)> GetDropdownDataAsync()
        {
            var loaiList = await _context.LoaiVatTus.OrderBy(l => l.TenLoaiVatTu).ToListAsync();
            var nccList = await _context.NhaCungCaps.OrderBy(n => n.TenNhaCungCap).ToListAsync();
            return (loaiList, nccList);
        }
    }
}