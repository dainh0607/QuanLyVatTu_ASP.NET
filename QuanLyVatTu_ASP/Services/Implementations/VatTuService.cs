using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.VatTu;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class VatTuService : IVatTuService
    {
        private readonly AppDbContext _context;

        public VatTuService(AppDbContext context)
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
                keyword = keyword.ToLower();
                query = query.Where(x =>
                    x.MaHienThi.ToLower().Contains(keyword) ||
                    x.TenVatTu.ToLower().Contains(keyword) ||
                    x.DonViTinh.ToLower().Contains(keyword) ||
                    (x.LoaiVatTu != null && x.LoaiVatTu.TenLoaiVatTu.ToLower().Contains(keyword)) ||
                    (x.NhaCungCap != null && x.NhaCungCap.TenNhaCungCap.ToLower().Contains(keyword)));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new VatTuIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi,
                    TenVatTu = x.TenVatTu,
                    DonViTinh = x.DonViTinh,
                    GiaNhap = x.GiaNhap ?? 0,
                    GiaBan = x.GiaBan ?? 0,
                    SoLuongTon = x.SoLuongTon ?? 0,
                    TenLoaiVatTu = x.LoaiVatTu != null ? x.LoaiVatTu.TenLoaiVatTu : "",
                    TenNhaCungCap = x.NhaCungCap != null ? x.NhaCungCap.TenNhaCungCap : "",
                    NgayTao = x.NgayTao
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

        public async Task<VatTuCreateEditViewModel?> GetByIdForEditAsync(int id)
        {
            var vt = await _context.VatTus
                    .Include(v => v.LoaiVatTu)
                    .Include(v => v.NhaCungCap)
                    .FirstOrDefaultAsync(x => x.ID == id);

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

        public async Task<string?> CreateAsync(VatTuCreateEditViewModel model)
        {
            if (await _context.VatTus.AnyAsync(x => x.TenVatTu == model.TenVatTu))
            {
                return "Tên vật tư này đã tồn tại trong kho.";
            }

            // Tạo mã hiển thị tự động - tìm mã bị thiếu
            string maHienThiAuto = await GetNextMaHienThiAsync();

            var vt = new VatTu
            {
                MaHienThi = maHienThiAuto,
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

        public async Task<string?> UpdateAsync(int id, VatTuCreateEditViewModel model)
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

        public async Task<string?> DeleteAsync(int id)
        {
            var vt = await _context.VatTus.FindAsync(id);
            if (vt == null) return "Vật tư không tồn tại";

            bool inDonHang = await _context.ChiTietDonHangs.AnyAsync(ct => ct.MaVatTu == id);
            bool inHoaDon = await _context.ChiTietHoaDons.AnyAsync(ct => ct.MaVatTu == id);

            if (inDonHang || inHoaDon)
            {
                return "Không thể xóa! Vật tư này đã phát sinh giao dịch (Đơn hàng/Hóa đơn).";
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

        /// <summary>
        /// Sinh mã hiển thị tiếp theo - tìm mã bị thiếu trong dãy VT001, VT002...
        /// Ví dụ: Nếu xóa VT003 thì mã mới sẽ là VT003
        /// </summary>
        public async Task<string> GetNextMaHienThiAsync()
        {
            // Lấy tất cả mã hiện có
            var existingCodes = await _context.VatTus
                .Select(x => x.MaHienThi)
                .Where(x => x.StartsWith("VT"))
                .ToListAsync();

            // Parse số từ mã
            var usedNumbers = existingCodes
                .Select(x => {
                    if (x.Length > 2 && int.TryParse(x.Substring(2), out int n))
                        return n;
                    return 0;
                })
                .Where(x => x > 0)
                .ToHashSet();

            // Tìm số nhỏ nhất chưa sử dụng
            int nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return $"VT{nextNumber:D3}"; // VT001, VT002...
        }
    }
}