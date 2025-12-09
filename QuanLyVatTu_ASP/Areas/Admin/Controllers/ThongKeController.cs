using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/thong-ke")]
    public class ThongKeController : AdminBaseController
    {
        private readonly IThongKeService _thongKeService;

        public ThongKeController(IThongKeService thongKeService)
        {
            _thongKeService = thongKeService;
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
            var model = await _thongKeService.GetDashboardStatsAsync(
                fromDate,
                toDate,
                status,
                paymentMethod,
                nhanVienId,
                khachHangId
            );

            var dropdowns = await _thongKeService.GetFilterDropdownsAsync(nhanVienId, khachHangId);

            ViewBag.NhanViens = dropdowns.NhanViens;
            ViewBag.KhachHangs = dropdowns.KhachHangs;
            ViewBag.TrangThais = dropdowns.TrangThais;
            ViewBag.PhuongThucs = dropdowns.PhuongThucs;

            return View(model);
        }
    }
}