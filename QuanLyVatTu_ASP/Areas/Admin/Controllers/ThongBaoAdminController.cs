using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/thong-bao")]
    public class ThongBaoAdminController : AdminBaseController
    {
        private readonly IThongBaoService _thongBaoService;
        private readonly IHangThanhVienService _hangThanhVienService;

        public ThongBaoAdminController(IThongBaoService thongBaoService, IHangThanhVienService hangThanhVienService)
        {
            _thongBaoService = thongBaoService;
            _hangThanhVienService = hangThanhVienService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách hạng để đưa vào Dropdown lọc khách nhận
            var tiers = await _hangThanhVienService.GetAllAsync();
            
            var tierOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "ALL", Text = "Tất cả khách hàng (đã đăng ký nhận tin)" }
            };

            foreach (var t in tiers)
            {
                tierOptions.Add(new SelectListItem { Value = t.ID.ToString(), Text = $"Khách hàng {t.TenHang}" });
            }

            ViewBag.DoiTuongNhan = tierOptions;
            return View();
        }

        [HttpPost("broadcast")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Broadcast(string TieuDe, string NoiDung, string? LinkDich, string DoiTuongNhan)
        {
            if (string.IsNullOrWhiteSpace(TieuDe) || string.IsNullOrWhiteSpace(NoiDung))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ Tiêu đề và Nội dung thông báo.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _thongBaoService.BroadcastNotificationAsync(TieuDe, NoiDung, LinkDich, DoiTuongNhan);
                TempData["Success"] = "Đã gửi thông báo hàng loạt thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi gửi thông báo: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
