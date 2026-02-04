using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class ChiTietDonHangService : IChiTietDonHangService
    {
        private readonly AppDbContext _context;

        public ChiTietDonHangService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ChiTietDonHangViewModel?> GetDetailViewModelAsync(int maDonHang, string search)
        {
            var donHangExists = await _context.DonHang.AnyAsync(d => d.ID == maDonHang);
            if (!donHangExists) return null;

            var query = _context.VatTus.Include(v => v.LoaiVatTu).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.ToLower();
                query = query.Where(v =>

                    
                    v.TenVatTu.ToLower().Contains(s) ||
                    (v.LoaiVatTu != null && v.LoaiVatTu.TenLoaiVatTu.ToLower().Contains(s)));
            }

            var dsVatTu = await query
                .Select(v => new VatTuSelectItem
                {
                    MaVatTu = v.ID,
                    MaCode = v.ID.ToString("VT000"),
                    TenVatTu = v.TenVatTu,
                    TenLoai = v.LoaiVatTu != null ? v.LoaiVatTu.TenLoaiVatTu : "Chưa phân loại",
                    SoLuongTon = v.SoLuongTon ?? 0,
                    GiaBan = v.GiaBan ?? 0
                })
                .ToListAsync();

            var chiTiet = await _context.ChiTietDonHangs
                .Include(c => c.VatTu)
                .Where(c => c.MaDonHang == maDonHang)
                .Select(c => new ChiTietDonHangItem
                {
                    MaVatTu = c.MaVatTu,
                    TenVatTu = c.VatTu.TenVatTu,
                    SoLuong = c.SoLuong ?? 0,
                    DonGia = c.DonGia ?? 0
                })
                .ToListAsync();
            var donHang = await _context.DonHang
                 .Include(d => d.KhachHang)
                 .FirstOrDefaultAsync(d => d.ID == maDonHang);

            var hoaDonId = await _context.HoaDons
                 .Where(h => h.MaDonHang == maDonHang)
                 .Select(h => (int?)h.ID)
                 .FirstOrDefaultAsync();

            return new ChiTietDonHangViewModel
            {
                MaDonHang = maDonHang,
                TenKhachHang = donHang?.KhachHang?.HoTen,
                NgayTao = donHang?.NgayTao,
                DanhSachVatTu = dsVatTu,
                ChiTietDonHang = chiTiet,
                SearchTerm = search,
                HoaDonId = hoaDonId,
                SoTienDatCoc = donHang?.SoTienDatCoc
            };
        }

        public async Task<string?> AddVatTuAsync(int maDonHang, int maVatTu, int soLuong)
        {
            if (maVatTu <= 0 || soLuong <= 0) return "Dữ liệu không hợp lệ";

            var vatTu = await _context.VatTus.FindAsync(maVatTu);
            if (vatTu == null) return "Vật tư không tồn tại";

            var exist = await _context.ChiTietDonHangs
                .FirstOrDefaultAsync(c => c.MaDonHang == maDonHang && c.MaVatTu == maVatTu);

            if (exist != null)
            {
                exist.SoLuong += soLuong;
            }
            else
            {
                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = maDonHang,
                    MaVatTu = maVatTu,
                    SoLuong = soLuong,
                    DonGia = vatTu.GiaBan
                });
            }

            await _context.SaveChangesAsync();

            await CapNhatTongTienDonHang(maDonHang);

            return null;
        }

        public async Task<string?> UpdateSoLuongAsync(int maDonHang, int maVatTu, int soLuong)
        {
            if (soLuong < 1) return "Số lượng phải ≥ 1";

            var ct = await _context.ChiTietDonHangs
                .FirstOrDefaultAsync(c => c.MaDonHang == maDonHang && c.MaVatTu == maVatTu);

            if (ct == null) return "Chi tiết đơn hàng không tìm thấy";

            ct.SoLuong = soLuong;
            await _context.SaveChangesAsync();

            await CapNhatTongTienDonHang(maDonHang);

            return null;
        }

        public async Task<string?> RemoveVatTuAsync(int maDonHang, List<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0) return "Chưa chọn vật tư nào để xóa";

            var items = await _context.ChiTietDonHangs
                .Where(c => c.MaDonHang == maDonHang && selectedIds.Contains(c.MaVatTu))
                .ToListAsync();

            if (items.Any())
            {
                _context.ChiTietDonHangs.RemoveRange(items);
                await _context.SaveChangesAsync();

                await CapNhatTongTienDonHang(maDonHang);
            }

            return null;
        }

        private async Task CapNhatTongTienDonHang(int maDonHang)
        {
            var donHang = await _context.DonHang.FindAsync(maDonHang);
            if (donHang != null)
            {
                var tongTien = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == maDonHang)
                    .SumAsync(ct => (ct.SoLuong ?? 0) * (ct.DonGia ?? 0));

                donHang.TongTien = tongTien;
                _context.Update(donHang);
                await _context.SaveChangesAsync();
            }
        }
    }
}