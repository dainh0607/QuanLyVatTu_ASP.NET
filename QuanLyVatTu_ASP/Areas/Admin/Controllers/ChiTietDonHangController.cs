using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu.Areas.Admin.Controllers;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/chi-tiet-don-hang")]
    public class ChiTietDonHangController : AdminBaseController
    {
        private readonly IChiTietDonHangService _chiTietService;

        public ChiTietDonHangController(IChiTietDonHangService chiTietService)
        {
            _chiTietService = chiTietService;
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id, string search = "")
        {
            var model = await _chiTietService.GetDetailViewModelAsync(id, search);

            if (model == null) return NotFound();

            return View("~/Areas/Admin/Views/DonHang/Details.cshtml", model);
        }

        [HttpPost("them-vat-tu")]
        public async Task<IActionResult> ThemVatTu(int maDonHang, int maVatTu, int soLuong = 1)
        {
            var error = await _chiTietService.AddVatTuAsync(maDonHang, maVatTu, soLuong);

            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Đã thêm vật tư vào đơn hàng";
            }

            return RedirectToAction(nameof(Details), new { id = maDonHang });
        }

        [HttpPost("sua-so-luong")]
        public async Task<IActionResult> SuaSoLuong(int maDonHang, int maVatTu, int soLuong)
        {
            var error = await _chiTietService.UpdateSoLuongAsync(maDonHang, maVatTu, soLuong);

            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Cập nhật số lượng thành công";
            }

            return RedirectToAction(nameof(Details), new { id = maDonHang });
        }

        [HttpPost("xoa-nhieu")]
        public async Task<IActionResult> XoaNhieu(int maDonHang, List<int> selectedIds)
        {
            var error = await _chiTietService.RemoveVatTuAsync(maDonHang, selectedIds);

            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Đã xóa vật tư được chọn";
            }

            return RedirectToAction(nameof(Details), new { id = maDonHang });
        }
    }
}