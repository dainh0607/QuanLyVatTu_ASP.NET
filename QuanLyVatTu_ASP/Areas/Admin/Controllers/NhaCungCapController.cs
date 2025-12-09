using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhaCungCap;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/nha-cung-cap")]
    public class NhaCungCapController : AdminBaseController
    {
        private readonly INhaCungCapService _nccService;

        public NhaCungCapController(INhaCungCapService nccService)
        {
            _nccService = nccService;
        }

        // GET: /admin/nha-cung-cap
        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            var model = await _nccService.GetAllPagingAsync(keyword, page, 10);
            ViewBag.Keyword = keyword;
            return View(model);
        }

        // GET: /admin/nha-cung-cap/them-moi
        [HttpGet("them-moi")]
        public IActionResult Create()
        {
            return View(new NhaCungCapCreateEditViewModel());
        }

        // POST: /admin/nha-cung-cap/them-moi
        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhaCungCapCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var error = await _nccService.CreateAsync(model);

                if (error != null)
                {
                    // Logic xử lý lỗi để hiển thị đúng chỗ
                    if (error.Contains("Email")) ModelState.AddModelError("Email", error);
                    else if (error.Contains("Tên")) ModelState.AddModelError("TenNhaCungCap", error);
                    else ModelState.AddModelError("", error);

                    return View(model);
                }

                TempData["Success"] = "Thêm nhà cung cấp thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /admin/nha-cung-cap/sua/5
        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _nccService.GetByIdForEditAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // POST: /admin/nha-cung-cap/sua/5
        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhaCungCapCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var error = await _nccService.UpdateAsync(id, model);

                if (error != null)
                {
                    if (error.Contains("Email")) ModelState.AddModelError("Email", error);
                    else if (error.Contains("Tên")) ModelState.AddModelError("TenNhaCungCap", error);
                    else ModelState.AddModelError("", error);

                    return View(model);
                }

                TempData["Success"] = "Cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: /admin/nha-cung-cap/xoa/5
        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var error = await _nccService.DeleteAsync(id);

            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Đã xóa nhà cung cấp";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}