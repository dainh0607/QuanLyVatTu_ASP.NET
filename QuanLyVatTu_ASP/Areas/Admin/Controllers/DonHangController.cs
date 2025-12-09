using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/don-hang")]
    public class DonHangController : AdminBaseController
    {
        private readonly IDonHangService _donHangService;

        public DonHangController(IDonHangService donHangService)
        {
            _donHangService = donHangService;
        }

        private async Task LoadDropdownData(int? selectedKhachHangId = null, int? selectedNhanVienId = null)
        {
            var khList = await _donHangService.GetKhachHangLookupAsync();
            var nvList = await _donHangService.GetNhanVienLookupAsync();

            ViewBag.KhachHangList = new SelectList(khList, "ID", "HoTen", selectedKhachHangId);
            ViewBag.NhanVienList = new SelectList(nvList, "ID", "HoTen", selectedNhanVienId);
        }

        [HttpGet("", Name = "AdminDonHang")]
        public async Task<IActionResult> Index(string keyword = "", int page = 1)
        {
            var model = await _donHangService.GetAllPagingAsync(keyword, page, 10);
            ViewBag.Keyword = keyword;
            return View(model);
        }

        [HttpGet("them-moi")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData(); // Load dropdown
            return View(new DonHangCreateEditViewModel
            {
                NgayDat = DateTime.Now,
                TrangThai = "Chờ xác nhận"
            });
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonHangCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _donHangService.CreateAsync(model);
                TempData["Success"] = "Tạo đơn hàng thành công";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi validate -> Load lại dropdown để không bị lỗi View
            await LoadDropdownData(model.KhachHangId, model.NhanVienId);
            return View(model);
        }

        [HttpGet("sua/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _donHangService.GetByIdForEditAsync(id);
            if (model == null) return NotFound();

            await LoadDropdownData(model.KhachHangId, model.NhanVienId);
            return View(model);
        }

        [HttpPost("sua/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DonHangCreateEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var success = await _donHangService.UpdateAsync(id, model);
                if (!success) return NotFound();

                TempData["Success"] = "Cập nhật đơn hàng thành công";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData(model.KhachHangId, model.NhanVienId);
            return View(model);
        }

        [HttpPost("xoa/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _donHangService.DeleteAsync(id);
            TempData["Success"] = "Đã xóa đơn hàng";
            return RedirectToAction(nameof(Index));
        }
    }
}