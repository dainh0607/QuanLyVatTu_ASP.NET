using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.KhachHangViewModels;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/khach-hang")]
    public class KhachHangController : AdminBaseController
    {
        private readonly IKhachHangService _khachHangService;

        public KhachHangController(IKhachHangService khachHangService)
        {
            _khachHangService = khachHangService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            var model = await _khachHangService.GetAllPagingAsync(keyword, page, 15);
            ViewBag.Keyword = keyword;
            return View(model);
        }

        [HttpGet("them-moi")]
        public IActionResult Create()
        {
            return View(new KhachHangCreateEditViewModel());
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachHangCreateEditViewModel model)
        {
            if (string.IsNullOrEmpty(model.MatKhau))
            {
                ModelState.AddModelError("MatKhau", "Vui lòng nhập mật khẩu");
            }
            if (!ModelState.IsValid) return View(model);

            // Gọi Service
            var errorMessage = await _khachHangService.CreateAsync(model);

            if (errorMessage != null)
            {
                ModelState.AddModelError("", errorMessage);
                return View(model);
            }

            TempData["Success"] = "Thêm khách hàng thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _khachHangService.GetByIdForEditAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhachHangCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            // Nếu không nhập mật khẩu -> Xóa validate để giữ pass cũ
            if (string.IsNullOrWhiteSpace(model.MatKhau)) ModelState.Remove("MatKhau");

            if (!ModelState.IsValid) return View(model);

            // Gọi Service
            var errorMessage = await _khachHangService.UpdateAsync(id, model);

            if (errorMessage != null)
            {
                ModelState.AddModelError("", errorMessage);
                return View(model);
            }

            TempData["Success"] = "Cập nhật khách hàng thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var errorMessage = await _khachHangService.DeleteAsync(id);

            if (errorMessage != null)
            {
                TempData["Error"] = errorMessage;
            }
            else
            {
                TempData["Success"] = "Đã xóa khách hàng";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}