using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhanVien;
using QuanLyVatTu_ASP.Helpers;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/nhan-vien")]
    public class NhanVienController : AdminBaseController
    {
        private readonly INhanVienService _nhanVienService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NhanVienController(INhanVienService nhanVienService, IWebHostEnvironment webHostEnvironment)
        {
            _nhanVienService = nhanVienService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            var model = await _nhanVienService.GetAllPagingAsync(keyword, page, 15);
            ViewBag.Keyword = keyword;
            return View(model);
        }

        [HttpGet("them-moi")]
        public async Task<IActionResult> Create()
        {
            var nextMa = await _nhanVienService.GetNextMaHienThiAsync();
            return View(new NhanVienCreateEditViewModel { NgaySinh = DateTime.Now.AddYears(-22), MaHienThi = nextMa });
        }

        // GET: /admin/nhan-vien/chi-tiet/5
        [HttpGet("chi-tiet/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _nhanVienService.GetByIdAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // API: /admin/nhan-vien/get-next-ma
        [HttpGet("get-next-ma")]
        public async Task<IActionResult> GetNextMaHienThi()
        {
            var nextMa = await _nhanVienService.GetNextMaHienThiAsync();
            return Json(new { maHienThi = nextMa });
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVienCreateEditViewModel model)
        {
            if (string.IsNullOrEmpty(model.MatKhau))
            {
                ModelState.AddModelError("MatKhau", "Vui lòng nhập mật khẩu");
            }
            if (!ModelState.IsValid) return View(model);

            // Xử lý upload ảnh
            if (model.AnhDaiDienFile != null)
            {
                model.AnhDaiDien = await FileUploadHelper.UploadFileAsync(
                    model.AnhDaiDienFile, _webHostEnvironment.WebRootPath, "images/nhanvien");
            }

            var errorMessage = await _nhanVienService.CreateAsync(model);

            if (errorMessage != null)
            {
                ModelState.AddModelError("", errorMessage);
                return View(model);
            }

            TempData["Success"] = "Thêm nhân viên thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _nhanVienService.GetByIdForEditAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhanVienCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.MatKhau)) ModelState.Remove("MatKhau");

            if (!ModelState.IsValid) return View(model);

            // Xử lý upload ảnh mới
            if (model.AnhDaiDienFile != null)
            {
                // Xóa ảnh cũ (nếu có)
                FileUploadHelper.DeleteFile(model.AnhDaiDien, _webHostEnvironment.WebRootPath);
                
                // Upload ảnh mới
                model.AnhDaiDien = await FileUploadHelper.UploadFileAsync(
                    model.AnhDaiDienFile, _webHostEnvironment.WebRootPath, "images/nhanvien");
            }

            var errorMessage = await _nhanVienService.UpdateAsync(id, model);

            if (errorMessage != null)
            {
                ModelState.AddModelError("", errorMessage);
                return View(model);
            }

            TempData["Success"] = "Cập nhật thông tin thành công";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var errorMessage = await _nhanVienService.DeleteAsync(id);

            if (errorMessage != null)
            {
                TempData["Error"] = errorMessage;
            }
            else
            {
                TempData["Success"] = "Đã xóa nhân viên";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}