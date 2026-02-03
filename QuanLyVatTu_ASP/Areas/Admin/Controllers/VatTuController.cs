using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.VatTu;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/vat-tu")]
    public class VatTuController : AdminBaseController
    {
        private readonly IVatTuService _vatTuService;

        public VatTuController(IVatTuService vatTuService)
        {
            _vatTuService = vatTuService;
        }

        // Hàm hỗ trợ load Dropdown
        private async Task PrepareViewBag(int? selectedLoai = null, int? selectedNCC = null)
        {
            var data = await _vatTuService.GetDropdownDataAsync();

            ViewBag.LoaiVatTuList = new SelectList(data.LoaiList, "ID", "TenLoaiVatTu", selectedLoai);
            ViewBag.NhaCungCapList = new SelectList(data.NccList, "ID", "TenNhaCungCap", selectedNCC);
        }

        // GET: /admin/vat-tu
        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            var model = await _vatTuService.GetAllPagingAsync(keyword, page, 15);
            ViewBag.Keyword = keyword;
            return View(model);
        }

        // GET: /admin/vat-tu/them-moi
        [HttpGet("them-moi")]
        public async Task<IActionResult> Create()
        {
            await PrepareViewBag();
            return View(new VatTuCreateEditViewModel());
        }

        // POST: /admin/vat-tu/them-moi
        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VatTuCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var error = await _vatTuService.CreateAsync(model);

                if (error != null)
                {
                    ModelState.AddModelError("TenVatTu", error);
                    await PrepareViewBag(model.MaLoaiVatTu, model.MaNhaCungCap);
                    return View(model);
                }

                TempData["Success"] = "Thêm vật tư thành công";
                return RedirectToAction(nameof(Index));
            }

            await PrepareViewBag(model.MaLoaiVatTu, model.MaNhaCungCap);
            return View(model);
        }

        // GET: /admin/vat-tu/sua/5
        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _vatTuService.GetByIdForEditAsync(id);
            if (model == null) return NotFound();

            await PrepareViewBag(model.MaLoaiVatTu, model.MaNhaCungCap);
            return View(model);
        }

        // POST: /admin/vat-tu/sua/5
        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VatTuCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var error = await _vatTuService.UpdateAsync(id, model);

                if (error != null)
                {
                    ModelState.AddModelError("TenVatTu", error);
                    await PrepareViewBag(model.MaLoaiVatTu, model.MaNhaCungCap);
                    return View(model);
                }

                TempData["Success"] = "Cập nhật vật tư thành công";
                return RedirectToAction(nameof(Index));
            }

            await PrepareViewBag(model.MaLoaiVatTu, model.MaNhaCungCap);
            return View(model);
        }

        // POST: /admin/vat-tu/xoa/5
        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var error = await _vatTuService.DeleteAsync(id);

            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Đã xóa vật tư khỏi kho";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}