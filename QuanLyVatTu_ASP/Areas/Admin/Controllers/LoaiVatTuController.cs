using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.LoaiVatTu;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/loai-vat-tu")]
    public class LoaiVatTuController : AdminBaseController
    {
        private readonly ILoaiVatTuService _loaiVatTuService;

        public LoaiVatTuController(ILoaiVatTuService loaiVatTuService)
        {
            _loaiVatTuService = loaiVatTuService;
        }

        // GET: /admin/loai-vat-tu
        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            var model = await _loaiVatTuService.GetAllPagingAsync(keyword, page, 15);
            ViewBag.Keyword = keyword;
            return View(model);
        }

        // GET: /admin/loai-vat-tu/them-moi
        [HttpGet("them-moi")]
        public IActionResult Create()
        {
            return View(new LoaiVatTuCreateEditViewModel());
        }

        // POST: /admin/loai-vat-tu/them-moi
        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoaiVatTuCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var error = await _loaiVatTuService.CreateAsync(model);
                if (error != null)
                {
                    ModelState.AddModelError("TenLoaiVatTu", error);
                    return View(model);
                }

                TempData["Success"] = "Thêm mới thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /admin/loai-vat-tu/sua/5
        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _loaiVatTuService.GetByIdForEditAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // POST: /admin/loai-vat-tu/sua/5
        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LoaiVatTuCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var error = await _loaiVatTuService.UpdateAsync(id, model);
                if (error != null)
                {
                    ModelState.AddModelError("TenLoaiVatTu", error);
                    return View(model);
                }

                TempData["Success"] = "Cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: /admin/loai-vat-tu/xoa/5
        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var error = await _loaiVatTuService.DeleteAsync(id);

            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Đã xóa loại vật tư";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}