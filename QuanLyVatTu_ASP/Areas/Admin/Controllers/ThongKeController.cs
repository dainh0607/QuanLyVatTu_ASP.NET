using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.ThongKe;
using QuanLyVatTu_ASP.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/thong-ke")]
    public class ThongKeController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /admin/thong-ke
        [HttpGet("")]
        public async Task<IActionResult> Index(
            DateTime? fromDate,
            DateTime? toDate,
            string? status,
            string? paymentMethod,
            int? nhanVienId,
            int? khachHangId)
        {
            var query = _context.DonHang
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien)
                .AsNoTracking()
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.NgayDat.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.NgayDat.Date <= toDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                query = query.Where(x => x.TrangThai == status);
            }

            if (!string.IsNullOrEmpty(paymentMethod) && paymentMethod != "Tất cả")
            {
                query = query.Where(x => x.PhuongThucDatCoc == paymentMethod);
            }

            if (nhanVienId.HasValue && nhanVienId > 0)
            {
                query = query.Where(x => x.NhanVienId == nhanVienId);
            }

            if (khachHangId.HasValue && khachHangId > 0)
            {
                query = query.Where(x => x.KhachHangId == khachHangId);
            }

            var rawData = await query.OrderByDescending(x => x.NgayDat).ToListAsync();

            var model = new DashboardViewModel
            {
                TotalRevenue = rawData.Sum(x => x.TongTien),
                TotalOrders = rawData.Count,

                PaidOrders = rawData.Count(x => x.TrangThai == "Hoàn thành" || x.TrangThai == "Đã thanh toán"),

                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                PaymentMethod = paymentMethod,
                NhanVienId = nhanVienId,
                KhachHangId = khachHangId
            };

            model.Orders = rawData.Select(x => new OrderStatisticItem
            {
                Id = x.ID,
                MaDH = x.MaHienThi ?? $"DH{x.ID:0000}",
                Ngay = x.NgayDat,
                KhachHang = x.KhachHang?.HoTen ?? "Khách vãng lai",
                NhanVien = x.NhanVien?.HoTen ?? "Chưa phân công",
                TongTien = x.TongTien,
                TrangThai = x.TrangThai ?? "Mới tạo"
            }).ToList();

            var chartGrouping = rawData
                .GroupBy(x => x.NgayDat.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    DateLabel = g.Key.ToString("MM-dd"),
                    DailyRevenue = g.Sum(x => x.TongTien)
                })
                .ToList();

            model.ChartLabels = chartGrouping.Select(x => x.DateLabel).ToList();
            model.ChartData = chartGrouping.Select(x => x.DailyRevenue).ToList();

            await PrepareViewBagData(nhanVienId, khachHangId);

            return View(model);
        }

        private async Task PrepareViewBagData(int? selectedNhanVien, int? selectedKhachHang)
        {
            var nhanViens = await _context.NhanViens
                .Select(x => new { x.ID, x.HoTen })
                .ToListAsync();
            ViewBag.NhanViens = new SelectList(nhanViens, "ID", "HoTen", selectedNhanVien);

            var khachHangs = await _context.KhachHangs
                .Select(x => new { x.ID, x.HoTen })
                .ToListAsync();
            ViewBag.KhachHangs = new SelectList(khachHangs, "ID", "HoTen", selectedKhachHang);

            ViewBag.TrangThais = new List<string> {
                "Chờ xác nhận", "Đã xác nhận", "Đang xử lý", "Hoàn thành", "Đã hủy", "Đã thanh toán", "Chưa thanh toán"
            };

            ViewBag.PhuongThucs = new List<string> {
                "Tiền mặt", "Chuyển khoản", "COD"
            };
        }
    }
}