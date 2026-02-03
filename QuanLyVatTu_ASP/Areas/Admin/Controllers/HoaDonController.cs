using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/hoa-don")]
    public class HoaDonController : AdminBaseController
    {
        private readonly IHoaDonService _hoaDonService;

        public HoaDonController(IHoaDonService hoaDonService)
        {
            _hoaDonService = hoaDonService;
        }

        // GET: /admin/hoa-don
        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            var model = await _hoaDonService.GetOrdersForIndexAsync(keyword, page, 15);
            ViewBag.Keyword = keyword;
            return View(model);
        }

        // POST: /admin/hoa-don/tao-moi/5
        [HttpPost("tao-moi/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInvoice(int id)
        {
            // Gọi Service để xử lý tạo hóa đơn
            var result = await _hoaDonService.CreateInvoiceFromOrderAsync(id);

            // Kiểm tra kết quả trả về
            if (result.Error != null)
            {
                // Nếu có lỗi (VD: Chưa đủ cọc, đã xuất rồi...)
                TempData["Error"] = result.Error;
                return RedirectToAction(nameof(Index));
            }

            // Nếu thành công
            TempData["Success"] = "Xuất hóa đơn thành công!";
            // Chuyển hướng đến trang Chi tiết của hóa đơn vừa tạo (dùng ID mới trả về)
            return RedirectToAction(nameof(Details), new { id = result.NewInvoiceId });
        }

        // GET: /admin/hoa-don/chi-tiet/5
        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _hoaDonService.GetInvoiceDetailAsync(id);

            if (model == null) return NotFound();

            return View(model);
        }
    }
}