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
            if (HttpContext.Session.GetString("Role") == "Admin")
                return RedirectToAction("Index", "Home", new { area = "Admin" });

            if (HttpContext.Session.GetString("Role") == "Customer")
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            // Tìm Nhân viên
            var nhanVien = _unitOfWork.NhanVienRepository.GetByLogin(email, password);
            if (nhanVien != null)
            {
                HttpContext.Session.SetString("UserName", nhanVien.HoTen);
                HttpContext.Session.SetString("Email", nhanVien.Email ?? "");

                string role = (nhanVien.VaiTro == "Quản lý") ? "Admin" : "Employee";
                HttpContext.Session.SetString("Role", role);

                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            // Tìm Khách hàng
            var khachHang = _unitOfWork.KhachHangRepository.GetByLogin(email, password);
            if (khachHang != null)
            {
                HttpContext.Session.SetString("UserName", khachHang.HoTen);
                HttpContext.Session.SetString("Email", khachHang.Email ?? "");
                HttpContext.Session.SetString("Role", "Customer");

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
                string taiKhoanTuDong = email.Split('@')[0];
                string randomSuffix = new Random().Next(1000, 9999).ToString();

                var newKhach = new QuanLyVatTu_ASP.Areas.Admin.Models.KhachHang
                {
                    HoTen = hoTen,
                    Email = email,
                    MatKhau = password,
                    DiaChi = "",
                    SoDienThoai = "",

                    MaHienThi = "KH" + randomSuffix,
                    TaiKhoan = taiKhoanTuDong,
                    NgayTao = DateTime.Now,

                    // --- ĐÂY LÀ ĐĂNG KÝ THƯỜNG, KHÔNG PHẢI GOOGLE ---
                    DangNhapGoogle = false
                };

                _unitOfWork.KhachHangRepository.Add(newKhach);
                _unitOfWork.Save();

                ViewBag.Success = "Đăng ký thành công! Vui lòng đăng nhập.";
                return View("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi đăng ký: " + ex.Message;
                if (ex.InnerException != null)
                {
                    ViewBag.Error += " | Chi tiết: " + ex.InnerException.Message;
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