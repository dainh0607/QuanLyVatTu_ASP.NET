using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Repositories;

namespace QuanLyVatTu_ASP.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        // Inject UnitOfWork để truy cập CSDL
        public AccountController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Auth()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // 1. GỌI HÀM TÌM NHÂN VIÊN VỪA VIẾT
            var nhanVien = _unitOfWork.NhanVienRepository.GetByLogin(email, password);

            if (nhanVien != null)
            {
                HttpContext.Session.SetString("UserName", nhanVien.HoTen);
                HttpContext.Session.SetString("Email", nhanVien.Email ?? "");

                string role = "Employee";
                if (nhanVien.VaiTro == "Quản lý") role = "Admin";

                HttpContext.Session.SetString("Role", role);

                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            // 2. GỌI HÀM TÌM KHÁCH HÀNG VỪA VIẾT
            var khachHang = _unitOfWork.KhachHangRepository.GetByLogin(email, password);

            if (khachHang != null)
            {
                HttpContext.Session.SetString("UserName", khachHang.HoTen);
                HttpContext.Session.SetString("Role", "Customer");

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // 3. THẤT BẠI
            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác";
            return View("Auth");
        }

        // Action Đăng xuất (Cần thiết khi dùng Session)
        public IActionResult Logout()
        {
            // Xóa toàn bộ Session
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("Role");

            // Quay về trang đăng nhập
            return RedirectToAction("Auth");
        }
    }
}