using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Areas.Admin.Models;
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
            // Kiểm tra đăng nhập
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem thông tin tài khoản.";
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin khách hàng từ database
            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("Login", "Account");
            }

            // Lấy đơn hàng của khách hàng
            var donHangs = await _unitOfWork.DonHangRepository.GetDonHangByKhachHangAsync(khachHang.ID);

            var viewModel = new ProfileViewModel
            {
                KhachHang = khachHang,
                DonHangs = donHangs,
                SoSanPhamDaMua = donHangs?.Sum(d => d.ChiTietDonHangs?.Sum(ct => ct.SoLuong) ?? 0) ?? 0
            };

            return View(viewModel);
        }
        public async Task<IActionResult> History()
        {
            // Kiểm tra đăng nhập
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem lịch sử mua hàng.";
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin khách hàng
            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("Login", "Account");
            }

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

        /// <summary>
        /// Cập nhật thông tin hồ sơ khách hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                // Lấy khách hàng theo ID hoặc email từ session
                KhachHang? khachHang = null;
                
                if (request.Id > 0)
                {
                    khachHang = await _unitOfWork.KhachHangRepository.GetByIdAsync(request.Id);
                }
                else
                {
                    var email = HttpContext.Session.GetString("Email");
                    if (!string.IsNullOrEmpty(email))
                    {
                        khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
                    }
                }

                if (khachHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng" });
                }

                // Cập nhật thông tin
                if (!string.IsNullOrWhiteSpace(request.HoTen))
                    khachHang.HoTen = request.HoTen.Trim();
                
                if (!string.IsNullOrWhiteSpace(request.SoDienThoai))
                    khachHang.SoDienThoai = request.SoDienThoai.Trim();
                
                if (!string.IsNullOrWhiteSpace(request.DiaChi))
                    khachHang.DiaChi = request.DiaChi.Trim();

                await _unitOfWork.KhachHangRepository.UpdateAsync(khachHang);

                // Cập nhật session username nếu đổi tên
                if (!string.IsNullOrWhiteSpace(request.HoTen))
                {
                    HttpContext.Session.SetString("UserName", request.HoTen.Trim());
                }

                return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        public IActionResult About()
        {

            return View();
        }
        public IActionResult PurchasePolicy()
        {

            return View();
        }

        /// <summary>
        /// Tra cứu đơn hàng theo mã
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TrackOrder(int? id)
        {
            if (!id.HasValue)
            {
                return View();
            }

            var donHang = await _unitOfWork.DonHangRepository.GetByIdAsync(id.Value);
            
            if (donHang == null)
            {
                ViewBag.Error = "Không tìm thấy đơn hàng với mã này";
                return View();
            }

            return View(donHang);
        }

        /// <summary>
        /// Hiển thị danh sách đơn hàng của khách hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            // Kiểm tra đăng nhập
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin khách hàng
            var khachHang = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (khachHang == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách đơn hàng
            var donHangs = await _unitOfWork.DonHangRepository.GetDonHangByKhachHangAsync(khachHang.ID);
            
            ViewBag.KhachHang = khachHang;
            return View(donHangs);
        }

    }
}