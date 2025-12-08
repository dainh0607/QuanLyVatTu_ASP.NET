using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels;
using QuanLyVatTu_ASP.DataAccess;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/hoa-don")]
    public class HoaDonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HoaDonController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.DonHang
                .Include(d => d.KhachHang)
                .OrderByDescending(d => d.NgayTao)
                .Select(d => new HoaDonViewModel.OrderForItem
                {
                    DonHangId = d.ID,
                    MaDonHang = d.MaHienThi,
                    TenKhachHang = d.KhachHang.HoTen,
                    NgayDat = d.NgayDat,
                    TongTien = d.TongTien,
                    SoTienDatCoc = d.SoTienDatCoc ?? 0,
                    HoaDonId = _context.HoaDons.Where(hd => hd.MaDonHang == d.ID).Select(hd => hd.ID).FirstOrDefault()
                })
                .ToListAsync();

            foreach (var item in orders)
            {
                if (item.HoaDonId == 0) item.HoaDonId = null;
            }

            return View(new HoaDonViewModel { Orders = orders });
        }

        [HttpPost("tao-moi/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInvoice(int id)
        {
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (donHang == null) return NotFound();

            if (await _context.HoaDons.AnyAsync(h => h.MaDonHang == id))
            {
                TempData["Error"] = "Đơn hàng này đã được xuất hóa đơn!";
                return RedirectToAction(nameof(Index));
            }

            decimal tyLe = donHang.TongTien > 0 ? ((donHang.SoTienDatCoc ?? 0) / donHang.TongTien) : 0;
            if (tyLe < 0.1m)
            {
                TempData["Error"] = $"Chưa đủ điều kiện! Khách mới cọc {tyLe:P0}. Cần tối thiểu 10%.";
                return RedirectToAction(nameof(Index));
            }

            var hoaDon = new HoaDon
            {
                MaDonHang = donHang.ID,
                MaNhanVien = donHang.NhanVienId ?? 1,
                MaKhachHang = donHang.KhachHangId ?? 0,
                NgayLap = DateTime.Now,
                TongTienTruocThue = donHang.TongTien,
                TyLeThueGTGT = 10,
                ChietKhau = 0,
                SoTienDatCoc = donHang.SoTienDatCoc ?? 0,
                PhuongThucThanhToan = donHang.PhuongThucDatCoc,
                TrangThai = "Đã xuất"
            };

            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync();

            var chiTietHoaDons = donHang.ChiTietDonHangs.Select(ct => new ChiTietHoaDon
            {
                MaHoaDon = hoaDon.ID,
                MaVatTu = ct.MaVatTu,
                SoLuong = ct.SoLuong,
                DonGia = ct.DonGia,
                ThanhTien = ct.DonGia * ct.SoLuong
            }).ToList();

            _context.ChiTietHoaDons.AddRange(chiTietHoaDons);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xuất hóa đơn thành công!";
            return RedirectToAction(nameof(Details), new { id = hoaDon.ID });
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var hd = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.VatTu) 
                .FirstOrDefaultAsync(h => h.ID == id);

            if (hd == null) return NotFound();

            var model = new HoaDonDetailViewModel
            {
                HoaDonId = hd.ID,
                MaHoaDon = $"HD{hd.ID:0000}",
                NgayLap = hd.NgayLap,
                TenKhachHang = hd.KhachHang.HoTen,
                DiaChiKhachHang = hd.KhachHang.DiaChi,
                PhuongThucThanhToan = hd.PhuongThucThanhToan,
                TongTienHang = hd.TongTienTruocThue,
                ThueGTGT = hd.TienThueGTGT ?? 0, // Trigger đã tính, lấy lên hiển thị
                TongThanhToan = hd.TongTienSauThue ?? 0, // Trigger đã tính
                ChiTiet = hd.ChiTietHoaDons.Select((ct, index) => new HoaDonDetailViewModel.ChiTietItem
                {
                    STT = index + 1,
                    TenVatTu = ct.VatTu.TenVatTu,
                    DVT = ct.VatTu.DonViTinh,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                }).ToList()
            };

            return View(model);
        }
    }
}