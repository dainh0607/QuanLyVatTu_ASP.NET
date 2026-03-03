using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class DonHangService : IDonHangService
    {
        private readonly AppDbContext _context;
        private readonly IVoucherService _voucherService;
        private readonly IDiemTichLuyService _diemTichLuyService;
        private readonly IThongBaoService _thongBaoService;

        public DonHangService(AppDbContext context, IVoucherService voucherService, IDiemTichLuyService diemTichLuyService, IThongBaoService thongBaoService)
        {
            _context = context;
            _voucherService = voucherService;
            _diemTichLuyService = diemTichLuyService;
            _thongBaoService = thongBaoService;
        }

        public async Task<DonHangIndexViewModel> GetAllPagingAsync(string keyword, string status, int page, int pageSize)
        {
            if (page < 1) page = 1;

            // Include bảng KhachHang và NhanVien để lấy tên hiển thị
            var query = _context.DonHang
                .AsNoTracking() // Tối ưu RAM cho View
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(x =>
                    (x.MaHienThi != null && x.MaHienThi.ToLower().Contains(keyword)) ||
                    (x.GhiChu != null && x.GhiChu.ToLower().Contains(keyword)) ||
                    (x.KhachHang != null && x.KhachHang.HoTen.ToLower().Contains(keyword)));
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                if (status == "Chưa hoàn thành")
                {
                    // Lọc các đơn chưa có trạng thái "Hoàn thành"
                    query = query.Where(x => x.TrangThai != "Hoàn thành");
                }
                else
                {
                    query = query.Where(x => x.TrangThai == status);
                }
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.NgayDat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DonHangIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi ?? "DH" + x.ID.ToString("0000"),
                    TenKhachHang = x.KhachHang != null ? x.KhachHang.HoTen : "",
                    TenNhanVien = x.NhanVien != null ? x.NhanVien.HoTen : "",
                    HinhAnhNhanVien = x.NhanVien != null ? x.NhanVien.AnhDaiDien : null,
                    NgayDat = x.NgayDat,
                    TongTien = x.TongTien ?? 0,
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

        public async Task<DonHangCreateEditViewModel?> GetByIdForEditAsync(int id)
        {
            var entity = await _context.DonHang.FindAsync(id);
            if (entity == null) return null;

            return new DonHangCreateEditViewModel
            {
                Id = entity.ID,
                MaHienThi = entity.MaHienThi ?? "",
                KhachHangId = entity.KhachHangId,
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
                MaHienThi = "DH" + new Random().Next(1000, 9999), 
                KhachHangId = model.KhachHangId,
                NhanVienId = model.NhanVienId,
                NgayDat = model.NgayDat,
                TongTien = model.TongTien,
                SoTienDatCoc = model.SoTienDatCoc,
                PhuongThucDatCoc = model.PhuongThucDatCoc,
                NgayDatCoc = (model.NgayDatCoc.HasValue && model.NgayDatCoc.Value.Year < 1753) ? null : model.NgayDatCoc,
                TrangThai = model.TrangThai ?? "Chờ xác nhận",
                GhiChu = model.GhiChu,
                NgayTao = DateTime.Now
            };

            // === NGHIỆP VỤ ĐẶT CỌC: Đơn >= 5 triệu bắt buộc cọc 10% ===
            decimal tongTien = entity.TongTien ?? 0;
            if (tongTien >= 5000000)
            {
                decimal tienCocToiThieu = tongTien * 0.1m;
                // Nếu Admin chưa nhập đủ tiền cọc thì auto set
                if ((entity.SoTienDatCoc ?? 0) < tienCocToiThieu)
                {
                    entity.SoTienDatCoc = tienCocToiThieu;
                }
                entity.TrangThai = "Chờ đặt cọc";
                entity.PhuongThucDatCoc = string.IsNullOrEmpty(entity.PhuongThucDatCoc) ? "Chuyển khoản (QR)" : entity.PhuongThucDatCoc;
                entity.GhiChu = (entity.GhiChu ?? "") + " | Đơn hàng >= 5tr, bắt buộc cọc 10%.";
            }

            _context.DonHang.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(int id, DonHangCreateEditViewModel model)
        {
            var entity = await _context.DonHang.FindAsync(id);
            if (entity == null) return false;

            // 1. Nếu đơn đã hủy, chỉ cho phép cập nhật Ghi chú
            if (entity.TrangThai == "Đã hủy")
            {
                if (model.TrangThai != "Đã hủy") return false; // Không cho phép khôi phục đơn đã hủy
                entity.GhiChu = model.GhiChu;
                await _context.SaveChangesAsync();
                return true;
            }

            // 2. Validate chuyển đổi trạng thái (chỉ được tiến tới, không được lùi)
            string currentStatus = entity.TrangThai ?? "Chờ xác nhận";
            string newStatus = model.TrangThai ?? "Chờ xác nhận";

            // Khởi tạo Transaction để bảo vệ việc cập nhật trạng thái và hoàn kho
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Nếu trạng thái thay đổi
                if (currentStatus != newStatus)
                {
                    int currentLevel = GetStatusLevel(currentStatus);
                    int newLevel = GetStatusLevel(newStatus);

                    if (newStatus == "Đã hủy")
                    {
                        // Không cho phép hủy đơn đã hoàn thành
                        if (currentStatus == "Hoàn thành")
                        {
                             return false; 
                        }
                        // Đã hủy rồi thì bỏ qua
                        if (currentStatus == "Đã hủy")
                        {
                             return false;
                        }
                        
                        // [CRITICAL FIX] Trả lại Tồn Kho khi Hủy Đơn Hàng
                        var chiTietList = await _context.ChiTietDonHangs
                            .Where(c => c.MaDonHang == id)
                            .ToListAsync();
                             
                        foreach (var item in chiTietList)
                        {
                             var vatTu = await _context.VatTus.FindAsync(item.MaVatTu);
                             if (vatTu != null)
                             {
                                  vatTu.SoLuongTon = (vatTu.SoLuongTon ?? 0) + (item.SoLuong ?? 0);
                                  _context.VatTus.Update(vatTu);
                             }
                        }

                        // [NEW] Hoàn Voucher + Điểm khi Hủy đơn
                        await _voucherService.HandleOrderCancelVoucherAsync(id, currentStatus);
                        await _diemTichLuyService.RefundPointsAsync(id);
                    }
                    else if (newStatus == "Hoàn thành")
                    {
                        if (newLevel < currentLevel) return false;

                        // [NEW] Tích điểm + Xét nâng hạng khi hoàn thành đơn hàng
                        decimal finalAmount = entity.TongTienThucTra ?? entity.TongTien ?? 0;
                        await _diemTichLuyService.EarnPointsAsync(entity.KhachHangId, id, finalAmount);
                        await _diemTichLuyService.EvaluateTierUpgradeAsync(entity.KhachHangId);
                    }
                    else if (newLevel < currentLevel)
                    {
                        // Không cho phép quay lui trạng thái
                        return false;
                    }

                    // Tự động sinh thông báo cho khách hàng khi thay đổi trạng thái
                    string title = "Cập nhật đơn hàng " + (entity.MaHienThi ?? $"#{entity.ID}");
                    string message = $"Đơn hàng của bạn đã được chuyển sang trạng thái: {newStatus}.";
                    await _thongBaoService.CreateOrderNotificationAsync(
                        entity.KhachHangId,
                        title,
                        message,
                        entity.ID
                    );
                }

            decimal datCocCu = entity.SoTienDatCoc ?? 0;
            decimal datCocMoi = model.SoTienDatCoc ?? 0;
            decimal tongTien = entity.TongTien ?? 0;
            decimal tienCocToiThieu = tongTien >= 5000000 ? tongTien * 0.1M : 0;

            if (datCocMoi < datCocCu)
            {
                return false;
            }
            // Nghiệp vụ: Đơn >= 5 triệu phải cọc >= 10%, bất kể trạng thái
            if (tongTien >= 5000000 && datCocMoi < tienCocToiThieu)
            {
                return false;
            }
            if (model.NgayDatCoc.HasValue && model.NgayDatCoc.Value.Year < 1753)
            {
                entity.NgayDatCoc = null;
            }
            else
            {
                entity.NgayDatCoc = model.NgayDatCoc; 
            }

            entity.KhachHangId = model.KhachHangId;
            entity.NhanVienId = model.NhanVienId;
            
            // Safeguard SqlDateTime overflow
            if (model.NgayDat.Year >= 1753)
            {
                entity.NgayDat = model.NgayDat;
            }

            
            entity.SoTienDatCoc = datCocMoi;

            entity.PhuongThucDatCoc = model.PhuongThucDatCoc;
            entity.NgayDatCoc = model.NgayDatCoc;
            entity.TrangThai = model.TrangThai ?? "Chờ xác nhận";
            entity.GhiChu = model.GhiChu;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.DonHang
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (entity == null) return false;

            // Chỉ cho phép xóa đơn hàng Chờ xác nhận và Đã xác nhận
            if (entity.TrangThai != "Chờ xác nhận" && entity.TrangThai != "Đã xác nhận")
                return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Xóa LichSuSuDungVoucher liên quan (FK Restrict)
                var lichSuVoucher = await _context.LichSuSuDungVouchers
                    .Where(l => l.MaDonHang == id)
                    .ToListAsync();
                if (lichSuVoucher.Any())
                {
                    foreach (var ls in lichSuVoucher)
                    {
                        var voucher = await _context.Vouchers.FindAsync(ls.MaVoucherGoc);
                        if (voucher != null && voucher.SoLuongDaDung > 0)
                            voucher.SoLuongDaDung--;

                        var viVoucher = await _context.ViVoucherKhachHangs
                            .FirstOrDefaultAsync(v => v.MaKhachHang == ls.MaKhachHang && v.MaVoucherGoc == ls.MaVoucherGoc);
                        if (viVoucher != null)
                            viVoucher.TrangThaiTrongVi = "AVAILABLE";
                    }
                    _context.LichSuSuDungVouchers.RemoveRange(lichSuVoucher);
                }

                // 2. Xóa LichSuTichDiem liên quan (FK Restrict)
                var lichSuDiem = await _context.LichSuTichDiems
                    .Where(l => l.MaDonHang == id)
                    .ToListAsync();
                if (lichSuDiem.Any())
                    _context.LichSuTichDiems.RemoveRange(lichSuDiem);

                // 3. Xóa HoaDon + ChiTietHoaDon liên quan (FK Restrict)
                var hoaDons = await _context.HoaDons
                    .Where(h => h.MaDonHang == id)
                    .ToListAsync();
                foreach (var hd in hoaDons)
                {
                    var chiTietHD = await _context.ChiTietHoaDons
                        .Where(c => c.MaHoaDon == hd.ID)
                        .ToListAsync();
                    if (chiTietHD.Any())
                        _context.ChiTietHoaDons.RemoveRange(chiTietHD);
                }
                if (hoaDons.Any())
                    _context.HoaDons.RemoveRange(hoaDons);

                // 4. Hoàn kho + Xóa chi tiết đơn hàng
                if (entity.ChiTietDonHangs != null && entity.ChiTietDonHangs.Any())
                {
                    foreach (var item in entity.ChiTietDonHangs)
                    {
                        var vatTu = await _context.VatTus.FindAsync(item.MaVatTu);
                        if (vatTu != null)
                        {
                            vatTu.SoLuongTon = (vatTu.SoLuongTon ?? 0) + (item.SoLuong ?? 0);
                            _context.VatTus.Update(vatTu);
                        }
                    }
                    _context.ChiTietDonHangs.RemoveRange(entity.ChiTietDonHangs);
                }

                // 5. Xóa đơn hàng
                _context.DonHang.Remove(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private int GetStatusLevel(string status)
        {
            return status.Trim() switch
            {
                "Chờ xác nhận" => 1,
                "Đã xác nhận" => 2,
                "Đang xử lý" => 3,
                "Đang giao hàng" => 4,
                "Hoàn thành" => 5,
                "Đã hủy" => 100, // Status cuối
                _ => 0
            };
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