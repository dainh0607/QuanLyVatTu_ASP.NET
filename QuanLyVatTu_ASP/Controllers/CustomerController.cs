using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Models.ViewModels;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Repositories.Implementations;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Profile()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            var donHangs = await _unitOfWork.DonHangRepository.GetDonHangByKhachHangAsync(khachHang.ID);

            var viewModel = new ProfileViewModel
            {
                KhachHang = khachHang,
                DonHangs = donHangs,
                SoSanPhamDaMua = donHangs.Sum(d => d.ChiTietDonHangs?.Sum(ct => ct.SoLuong) ?? 0)
            };

            return View(viewModel);
        }
        public async Task<IActionResult> History()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);

            var orders = await _unitOfWork.DonHangRepository.GetDonHangByKhachHangAsync(khachHang.ID);

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }
            var khachHang = await _unitOfWork.KhachHangRepository.GetByIdAsync(model.Id);

            if (khachHang == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khách hàng" });
            }
            if (khachHang.MatKhau != model.MatKhauCu)
            {
                return Json(new { success = false, message = "Mật khẩu cũ không chính xác" });
            }

            khachHang.MatKhau = model.MatKhauMoi;

            await _unitOfWork.KhachHangRepository.UpdateAsync(khachHang);

            return Json(new
            {
                success = true,
                message = "Đổi mật khẩu thành công"
            });
        }

    }
}