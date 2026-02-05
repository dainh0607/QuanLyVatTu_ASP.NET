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

        public DonHangService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DonHangIndexViewModel> GetAllPagingAsync(string keyword, string status, int page, int pageSize)
        {
            if (page < 1) page = 1;

            // Include bảng KhachHang và NhanVien để lấy tên hiển thị
            var query = _context.DonHang
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
                if (status == "Chưa thanh toán")
                {
                    // Lọc các đơn chưa có trạng thái "Đã thanh toán"
                    query = query.Where(x => x.TrangThai != "Đã thanh toán");
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

            // Nếu trạng thái thay đổi
            if (currentStatus != newStatus)
            {
                int currentLevel = GetStatusLevel(currentStatus);
                int newLevel = GetStatusLevel(newStatus);

                // Đặc biệt: Nếu muốn Hủy đơn (level 0 hoặc -1) thì OK (trừ khi đã thanh toán/giao hàng?)
                // Yêu cầu: "Đã giao" thì không sửa lại được "Đã hủy"?, "Đã thanh toán" thì không sửa lại được "Đã hủy"
                if (newStatus == "Đã hủy")
                {
                    if (currentStatus == "Đã giao" || currentStatus == "Đã thanh toán")
                    {
                         // Không cho hủy khi đã giao/thanh toán (theo yêu cầu)
                         return false; 
                    }
                }
                else if (newLevel < currentLevel)
                {
                    // Không cho phép quay lui trạng thái
                    return false;
                }
            }

            decimal datCocCu = entity.SoTienDatCoc ?? 0;
            decimal datCocMoi = model.SoTienDatCoc ?? 0;
            decimal tienCocToiThieu = (entity.TongTien ?? 0) * 0.1M;

            if (datCocMoi < datCocCu)
            {
                return false;
            }
            if (model.TrangThai == "Đã xác nhận" && datCocMoi < tienCocToiThieu)
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
            return true;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.DonHang.FindAsync(id);
            if (entity != null)
            {
                // Chỉ cho phép xóa đơn hàng Chờ xác nhận và Đã xác nhận
                if (entity.TrangThai == "Chờ xác nhận" || entity.TrangThai == "Đã xác nhận")
                {
                    _context.DonHang.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private int GetStatusLevel(string status)
        {
            return status.Trim() switch
            {
                "Chờ xác nhận" => 1,
                "Đã xác nhận" => 2,
                "Đang xử lý" => 3,
                "Đã giao" => 4,
                "Đã thanh toán" => 5,
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