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
                    DonGia = c.DonGia ?? 0,
                    SoLuongTon = c.VatTu.SoLuongTon ?? 0
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
                SoTienDatCoc = donHang?.SoTienDatCoc,
                TrangThai = donHang?.TrangThai ?? ""
            };
        }

        public async Task<string?> AddVatTuAsync(int maDonHang, int maVatTu, int soLuong)
        {
            if (maVatTu <= 0 || soLuong <= 0) return "Dữ liệu không hợp lệ";

            var donHang = await _context.DonHang.FindAsync(maDonHang);
            if(donHang == null) return "Đơn hàng không tồn tại";

            if(donHang.TrangThai == "Đã hủy" || donHang.TrangThai == "Đang giao hàng" || donHang.TrangThai == "Hoàn thành")
                return "Không thể thay đổi chi tiết khi Đơn hàng ở trạng thái này.";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var vatTu = await _context.VatTus.FindAsync(maVatTu);
                if (vatTu == null) return "Vật tư không tồn tại";

                if (soLuong > (vatTu.SoLuongTon ?? 0)) return $"Số lượng không được vượt quá số lượng tồn ({vatTu.SoLuongTon ?? 0})";

                var exist = await _context.ChiTietDonHangs
                    .FirstOrDefaultAsync(c => c.MaDonHang == maDonHang && c.MaVatTu == maVatTu);

                if (exist != null)
                {
                    if (soLuong > (vatTu.SoLuongTon ?? 0)) return $"Số lượng bổ sung ({soLuong}) vượt tồn kho ({vatTu.SoLuongTon ?? 0})";
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

                // [CRITICAL FIX] - Admin nhét thêm vật tư vào đơn -> Trừ kho
                vatTu.SoLuongTon -= soLuong;
                _context.VatTus.Update(vatTu);

                await _context.SaveChangesAsync();
                await CapNhatTongTienDonHang(maDonHang);
                
                await transaction.CommitAsync();
                return null;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<string?> UpdateSoLuongAsync(int maDonHang, int maVatTu, int soLuong)
        {
            if (soLuong < 1) return "Số lượng phải ≥ 1";
            var donHang = await _context.DonHang.FindAsync(maDonHang);
            if (donHang == null) return "Đơn không tồn tại";
            if(donHang.TrangThai == "Đã hủy" || donHang.TrangThai == "Đang giao hàng" || donHang.TrangThai == "Hoàn thành")
                return "Không thể thay đổi chi tiết đơn hàng lúc này.";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ct = await _context.ChiTietDonHangs
                    .Include(c => c.VatTu)
                    .FirstOrDefaultAsync(c => c.MaDonHang == maDonHang && c.MaVatTu == maVatTu);

                if (ct == null) return "Chi tiết đơn hàng không tìm thấy";
                
                // Tính độ lệch (Delta) = Sl mới - Sl cũ
                int delta = soLuong - (ct.SoLuong ?? 0);
                
                // Trừ thêm kho nếu tăng SL (delta > 0). Cộng lại kho nếu giảm SL (delta < 0)
                if (delta > 0 && delta > (ct.VatTu?.SoLuongTon ?? 0)) 
                    return $"Vật tư không đủ hàng (Delta={delta} > Tồn={ct.VatTu?.SoLuongTon ?? 0})";

                ct.SoLuong = soLuong;
                
                if (ct.VatTu != null)
                {
                    ct.VatTu.SoLuongTon -= delta; // [FIX KHO]
                    _context.VatTus.Update(ct.VatTu);
                }

                await _context.SaveChangesAsync();
                await CapNhatTongTienDonHang(maDonHang);
                await transaction.CommitAsync();

                return null;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<string?> RemoveVatTuAsync(int maDonHang, List<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0) return "Chưa chọn vật tư nào để xóa";
            var donHang = await _context.DonHang.FindAsync(maDonHang);
            if (donHang == null) return "Đơn không tồn tại";
            if(donHang.TrangThai == "Đã hủy" || donHang.TrangThai == "Đang giao hàng" || donHang.TrangThai == "Hoàn thành")
                return "Không thể xóa chi tiết đơn hàng ở trạng thái này.";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var items = await _context.ChiTietDonHangs
                    .Include(c => c.VatTu)
                    .Where(c => c.MaDonHang == maDonHang && selectedIds.Contains(c.MaVatTu))
                    .ToListAsync();

                if (items.Any())
                {
                    foreach (var item in items)
                    {
                        if (item.VatTu != null)
                        {
                            item.VatTu.SoLuongTon += (item.SoLuong ?? 0); // [FIX KHO] Cộng hoàn trả
                            _context.VatTus.Update(item.VatTu);
                        }
                    }

                    _context.ChiTietDonHangs.RemoveRange(items);
                    await _context.SaveChangesAsync();

                    await CapNhatTongTienDonHang(maDonHang);
                }

                await transaction.CommitAsync();
                return null;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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