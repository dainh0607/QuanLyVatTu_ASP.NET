using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories;
using System.Text.RegularExpressions;

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
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewBag.Error = "Địa chỉ email không hợp lệ.";
                return View();
            }
            if (password.Length < 6)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự.";
                return View();
            }
            // 1. Tìm Nhân viên => COMMENT LẠI THEO YÊU CẦU
            /*
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
            */

            // --- GÁN CỨNG TÀI KHOẢN THEO YÊU CẦU ---

            // Admin: admin@gmail.com / 123456
            if (email == "admin@gmail.com" && password == "123456")
            {
                HttpContext.Session.SetString("UserName", "Admin User");
                HttpContext.Session.SetString("Email", email);
                HttpContext.Session.SetString("Role", "Admin");
                return RedirectToRoute("AdminDonHang");
            }

            // Client: client@gmail.com / 123456
            if (email == "client@gmail.com" && password == "123456")
            {
                HttpContext.Session.SetString("UserName", "Client User");
                HttpContext.Session.SetString("Email", email);
                HttpContext.Session.SetString("Role", "Customer");
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác (Hardcoded Check)";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string hoTen, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(hoTen) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "Vui lòng điền đầy đủ thông tin đăng ký.";
                return View("Login");
            }
            if (password.Length < 6)
            {
                ViewBag.Error = "Mật khẩu phải có tối thiểu 6 ký tự.";
                return View("Login");
            }

            // Kiểm tra ký tự đặc biệt trong Tên
            if (Regex.IsMatch(hoTen, @"[!@#$%^&*(),.?""':{}|<>]"))
            {
                ViewBag.Error = "Họ tên không được chứa ký tự đặc biệt.";
                return View("Login");
            }
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View("Login");
            }

            try
            {
                // var khachCu = _unitOfWork.KhachHangRepository.GetByLogin(email, ""); // Có thể cần comment check trùng nếu bỏ DB, nhưng user chỉ nói bỏ check mật khẩu/login. Để lại check trùng email cũng được, hoặc comment nốt. 
                // Tạm thời user bảo "lấy thông tin ... từ DB" -> có thể hiểu là Login. Register vẫn cần insert?
                // "comment lại các thành phần liên quan đến mã hóa mật khẩu" -> Done below.
                
                var khachCu = _unitOfWork.KhachHangRepository.GetByLogin(email, "");
                string taiKhoanTuDong = email.Split('@')[0];
                var newKhach = new QuanLyVatTu_ASP.Areas.Admin.Models.KhachHang
                {
                    HoTen = hoTen,
                    Email = email,
                    // MatKhau = BCrypt.Net.BCrypt.HashPassword(password), // COMMENT MÃ HÓA
                    MatKhau = password, // Lưu plain text
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