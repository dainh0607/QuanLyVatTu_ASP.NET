using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.VatTu;
using QuanLyVatTu_ASP.Helpers;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/vat-tu")]
    public class VatTuController : AdminBaseController
    {
        private readonly IVatTuService _vatTuService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VatTuController(IVatTuService vatTuService, IWebHostEnvironment webHostEnvironment)
        {
            _vatTuService = vatTuService;
            _webHostEnvironment = webHostEnvironment;
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
            var nextMa = await _vatTuService.GetNextMaHienThiAsync();
            return View(new VatTuCreateEditViewModel { MaHienThi = nextMa });
        }

        // API: /admin/vat-tu/get-next-ma
        [HttpGet("get-next-ma")]
        public async Task<IActionResult> GetNextMaHienThi()
        {
            var nextMa = await _vatTuService.GetNextMaHienThiAsync();
            return Json(new { maHienThi = nextMa });
        }

        // POST: /admin/vat-tu/them-moi
        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VatTuCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (model.HinhAnhFile != null)
                {
                    model.HinhAnh = await FileUploadHelper.UploadFileAsync(
                        model.HinhAnhFile, _webHostEnvironment.WebRootPath, "images/vattu");
                }

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
                // Xử lý upload ảnh mới
                if (model.HinhAnhFile != null)
                {
                    // Xóa ảnh cũ (nếu có)
                    FileUploadHelper.DeleteFile(model.HinhAnh, _webHostEnvironment.WebRootPath);
                    
                    // Upload ảnh mới
                    model.HinhAnh = await FileUploadHelper.UploadFileAsync(
                        model.HinhAnhFile, _webHostEnvironment.WebRootPath, "images/vattu");
                }

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