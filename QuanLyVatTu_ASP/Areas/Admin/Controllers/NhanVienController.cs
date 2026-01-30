using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhanVien;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/nhan-vien")]
    public class NhanVienController : AdminBaseController
    {
        private readonly INhanVienService _nhanVienService;

        public NhanVienController(INhanVienService nhanVienService)
        {
            _nhanVienService = nhanVienService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            var model = await _nhanVienService.GetAllPagingAsync(keyword, page, 10);
            ViewBag.Keyword = keyword;
            return View(model);
        }

        [HttpGet("them-moi")]
        public IActionResult Create()
        {
            return View(new NhanVienCreateEditViewModel { NgaySinh = DateTime.Now.AddYears(-22) });
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