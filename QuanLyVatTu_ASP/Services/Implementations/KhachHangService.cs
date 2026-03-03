using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.KhachHangViewModels;
using QuanLyVatTu_ASP.DataAccess;
using QuanLyVatTu_ASP.Services.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt;

namespace QuanLyVatTu_ASP.Services.Implementations
{
    public class KhachHangService : IKhachHangService
    {
        private readonly AppDbContext _context;

        public KhachHangService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<KhachHangIndexViewModel> GetAllPagingAsync(string keyword, int page, int pageSize)
        {
            if (page < 1) page = 1;
            var query = _context.KhachHangs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(x =>
                    x.MaHienThi.ToLower().Contains(keyword) ||
                    x.HoTen.ToLower().Contains(keyword) ||
                    x.Email.ToLower().Contains(keyword) ||
                    (x.SoDienThoai != null && x.SoDienThoai.ToLower().Contains(keyword)));
            }

            var total = await query.CountAsync();
            var items = await query
                .Include(x => x.HangThanhVien)
                .OrderByDescending(x => x.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new KhachHangIndexViewModel.ItemViewModel
                {
                    ID = x.ID,
                    MaHienThi = x.MaHienThi,
                    HoTen = x.HoTen,
                    Email = x.Email,
                    SoDienThoai = x.SoDienThoai,
                    SoNhaTenDuong = x.SoNhaTenDuong,
                    PhuongXa = x.PhuongXa,
                    TinhThanhPho = x.TinhThanhPho,
                    NgayTao = x.NgayTao,
                    DangNhapGoogle = x.DangNhapGoogle,
                    DiemTichLuy = x.DiemTichLuy,
                    TenHangThanhVien = x.HangThanhVien != null ? x.HangThanhVien.TenHang : null
                })
                .ToListAsync();

            return new KhachHangIndexViewModel
            {
                Items = items,
                PageIndex = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<KhachHangCreateEditViewModel?> GetByIdForEditAsync(int id)
        {
            var kh = await _context.KhachHangs
                .Include(x => x.HangThanhVien)
                .FirstOrDefaultAsync(x => x.ID == id);
            if (kh == null) return null;

            return new KhachHangCreateEditViewModel
            {
                Id = kh.ID,
                HoTen = kh.HoTen,
                Email = kh.Email,
                SoDienThoai = kh.SoDienThoai,
                SoNhaTenDuong = kh.SoNhaTenDuong,
                PhuongXa = kh.PhuongXa,
                TinhThanhPho = kh.TinhThanhPho,
                TaiKhoan = kh.TaiKhoan,
                AnhDaiDien = kh.AnhDaiDien,
                MaHienThi = kh.MaHienThi,
                DangNhapGoogle = kh.DangNhapGoogle,
                DiemTichLuy = kh.DiemTichLuy,
                TenHangThanhVien = kh.HangThanhVien?.TenHang,
                NgayLenHang = kh.NgayLenHang,
                NgayHetHanHang = kh.NgayHetHanHang
            };
        }

        public async Task<string?> CreateAsync(KhachHangCreateEditViewModel model)
        {
            if (await _context.KhachHangs.AnyAsync(x => x.Email == model.Email))
                return "Email này đã tồn tại.";

            if (await _context.KhachHangs.AnyAsync(x => x.TaiKhoan == model.TaiKhoan))
                return "Tài khoản này đã tồn tại.";

            if (!string.IsNullOrEmpty(model.SoDienThoai) && await _context.KhachHangs.AnyAsync(x => x.SoDienThoai == model.SoDienThoai))
                return "Số điện thoại này đã tồn tại.";

            string maHienThiAuto = await GetNextMaHienThiAsync();

            var kh = new KhachHang
            {
                MaHienThi = maHienThiAuto,
                HoTen = model.HoTen,
                Email = model.Email ?? "",
                SoDienThoai = model.SoDienThoai ?? "",
                SoNhaTenDuong = model.SoNhaTenDuong,
                PhuongXa = model.PhuongXa,
                TinhThanhPho = model.TinhThanhPho,
                TaiKhoan = model.TaiKhoan,
                AnhDaiDien = model.AnhDaiDien,

                // MatKhau = BCryptNet.HashPassword(model.MatKhau),
                MatKhau = model.MatKhau ?? "",

                DangNhapGoogle = false,
                NgayTao = DateTime.Now
            };

            _context.KhachHangs.Add(kh);
            await _context.SaveChangesAsync();

            return null;
        }

        public async Task<string?> UpdateAsync(int id, KhachHangCreateEditViewModel model)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return "Khách hàng không tồn tại.";

            if (await _context.KhachHangs.AnyAsync(x => x.Email == model.Email && x.ID != id))
                return "Email đã được sử dụng bởi khách hàng khác.";

            if (await _context.KhachHangs.AnyAsync(x => x.TaiKhoan == model.TaiKhoan && x.ID != id))
                return "Tài khoản đã được sử dụng bởi khách hàng khác.";

            if (!string.IsNullOrEmpty(model.SoDienThoai) && await _context.KhachHangs.AnyAsync(x => x.SoDienThoai == model.SoDienThoai && x.ID != id))
                return "SĐT đã được sử dụng bởi khách hàng khác.";

            kh.HoTen = model.HoTen;
            kh.Email = model.Email ?? "";
            kh.SoDienThoai = model.SoDienThoai ?? "";
            kh.SoNhaTenDuong = model.SoNhaTenDuong;
            kh.PhuongXa = model.PhuongXa;
            kh.TinhThanhPho = model.TinhThanhPho;
            kh.TaiKhoan = model.TaiKhoan;
            
            // Cập nhật ảnh nếu có
            if (!string.IsNullOrEmpty(model.AnhDaiDien))
            {
                kh.AnhDaiDien = model.AnhDaiDien;
            }

            if (!string.IsNullOrEmpty(model.MatKhau))
            {
                // kh.MatKhau = BCryptNet.HashPassword(model.MatKhau);
                kh.MatKhau = model.MatKhau ?? "";
            }

            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) return "Khách hàng không tồn tại";

            bool hasDonHang = await _context.DonHang.AnyAsync(d => d.KhachHangId == id);
            if (hasDonHang) return "Không thể xóa! Khách hàng này đã có đơn hàng.";

            _context.KhachHangs.Remove(kh);
            await _context.SaveChangesAsync();
            return null;
        }

        /// <summary>
        /// Sinh mã hiển thị tiếp theo - tìm mã bị thiếu trong dãy KH001, KH002...
        /// </summary>
        public async Task<string> GetNextMaHienThiAsync()
        {
            var existingCodes = await _context.KhachHangs
                .Select(x => x.MaHienThi)
                .Where(x => x.StartsWith("KH"))
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

            return $"KH{nextNumber:D3}";
        }

        public async Task<KhachHang?> GetByIdAsync(int id)
        {
            return await _context.KhachHangs
                .Include(x => x.HangThanhVien)
                .FirstOrDefaultAsync(x => x.ID == id);
        }

        public async Task<string?> DieuChinhDiemAsync(int khachHangId, int soDiemThayDoi, string lyDo)
        {
            if (string.IsNullOrWhiteSpace(lyDo))
                return "Cần nhập lý do điều chỉnh điểm.";

            if (soDiemThayDoi == 0)
                return "Số điểm thay đổi phải khác 0.";

            var kh = await _context.KhachHangs.FindAsync(khachHangId);
            if (kh == null) return "Không tìm thấy khách hàng.";

            // Backup điểm cũ
            int oldDiemGiaTichLuy = kh.DiemGiaTichLuy ?? 0;
            int oldDiemTichLuy = kh.DiemTichLuy ?? 0;

            // Xử lý cộng/trừ 
            // - DiemTichLuy: là số dư tài khoản dùng để chi tiêu (=> Không thể rớt xuống < 0)
            // - DiemGiaTichLuy: là tổng số điểm tích lũy dùng để xếp hạng (=> Nếu trừ thì có bị giảm không? Thông thường điểm hạng chỉ tăng, nhưng nếu phạt gian lận thì có thể trừ)
            
            // Xử lý điểm chi tiêu (DiemTichLuy)
            kh.DiemTichLuy = oldDiemTichLuy + soDiemThayDoi;
            if (kh.DiemTichLuy < 0) kh.DiemTichLuy = 0; // Không cho phép điểm xài âm

            // Xử lý điểm xét hạng (DiemGiaTichLuy)
            kh.DiemGiaTichLuy = oldDiemGiaTichLuy + soDiemThayDoi;
            if (kh.DiemGiaTichLuy < 0) kh.DiemGiaTichLuy = 0;

            // --- Ghi log Lịch Sử Điểm ---
            var lichSu = new QuanLyVatTu_ASP.Areas.Admin.Models.LichSuDiem
            {
                KhachHangId = kh.ID,
                SoDiem = soDiemThayDoi, // Lưu dấu + hoặc -
                LyDo = $"[Admin Điều chỉnh] {lyDo}",
                NgayTao = DateTime.Now
            };
            await _context.LichSuDiem.AddAsync(lichSu);
            await _context.SaveChangesAsync();
            
            // --- Gọi Update Tier ---
            // Vì không thể resolve IDiemTichLuyService (tránh vòng lặp inject), 
            // ta xử lý cập nhật hạng nhanh tại đây thay vì gọi IDiemTichLuyService.UpdateTierAsync
            await UpdateTierLocalAsync(kh);

            return null; // Success
        }

        private async Task UpdateTierLocalAsync(KhachHang khachHang)
        {
            var tiers = await _context.HangThanhViens.OrderBy(t => t.DiemToiThieu).ToListAsync();
            HangThanhVien? newTier = null;
            int totalDiem = khachHang.DiemGiaTichLuy ?? 0;

            foreach (var t in tiers)
            {
                if (totalDiem >= t.DiemToiThieu) newTier = t;
            }

            if (newTier != null && khachHang.MaHangThanhVien != newTier.ID)
            {
                khachHang.MaHangThanhVien = newTier.ID;
                khachHang.NgayLenHang = DateTime.Now;
                khachHang.NgayHetHanHang = DateTime.Now.AddYears(1);
                await _context.SaveChangesAsync();
            }
        }
    }
}