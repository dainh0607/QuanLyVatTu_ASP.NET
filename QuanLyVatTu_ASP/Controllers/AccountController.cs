using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories;

namespace QuanLyVatTu_ASP.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì chuyển hướng
            // Admin/Employee -> Vào trang Quản lý đơn hàng
            if (HttpContext.Session.GetString("Role") == "Admin" || HttpContext.Session.GetString("Role") == "Employee")
                return RedirectToRoute("AdminDonHang"); // <-- Dùng RedirectToRoute

            // Customer -> Vào trang chủ
            if (HttpContext.Session.GetString("Role") == "Customer")
                return RedirectToAction("Index", "Home", new { area = "" }); // <-- Customer về Home

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            // 1. Tìm Nhân viên
            var nhanVien = _unitOfWork.NhanVienRepository.GetByLogin(email, password);
            if (nhanVien != null)
            {
                HttpContext.Session.SetString("UserName", nhanVien.HoTen);
                HttpContext.Session.SetString("Email", nhanVien.Email ?? "");

                string role = (nhanVien.VaiTro == "Quản lý") ? "Admin" : "Employee";
                HttpContext.Session.SetString("Role", role);

                return RedirectToRoute("AdminDonHang");
            }

            // 2. Tìm Khách hàng
            var khachHang = _unitOfWork.KhachHangRepository.GetByLogin(email, password);
            if (khachHang != null)
            {
                HttpContext.Session.SetString("UserName", khachHang.HoTen);
                HttpContext.Session.SetString("Email", khachHang.Email ?? "");
                HttpContext.Session.SetString("Role", "Customer");

                // Khách hàng thì về Trang chủ (Home) chứ không vào Admin
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string hoTen, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View("Login");
            }

            try
            {
                var khachCu = _unitOfWork.KhachHangRepository.GetByLogin(email, "");
                string taiKhoanTuDong = email.Split('@')[0];
                var newKhach = new QuanLyVatTu_ASP.Areas.Admin.Models.KhachHang
                {
                    HoTen = hoTen,
                    Email = email,
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(password),
                    DiaChi = "",
                    SoDienThoai = "",
                    TaiKhoan = taiKhoanTuDong,
                    NgayTao = DateTime.Now,
                    DangNhapGoogle = false
                };

                _unitOfWork.KhachHangRepository.Add(newKhach);
                _unitOfWork.Save();

                ViewBag.Success = "Đăng ký thành công! Vui lòng đăng nhập.";
                return View("Login");
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("UNIQUE KEY constraint"))
                {
                    ViewBag.Error = "Email này đã được sử dụng. Vui lòng chọn email khác!";
                }
                else
                {
                    ViewBag.Error = "Lỗi đăng ký: " + ex.Message;
                }

                return View("Login");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}