using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using QuanLyVatTu_ASP.Areas.Admin.Models;
using QuanLyVatTu_ASP.Repositories;
using System.Security.Claims;
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
            // 1. Tìm Khách hàng trong database
            var khachHang = _unitOfWork.KhachHangRepository.GetByLogin(email, password);
            if (khachHang != null)
            {
                HttpContext.Session.SetString("UserName", khachHang.HoTen);
                HttpContext.Session.SetString("Email", khachHang.Email ?? "");
                HttpContext.Session.SetInt32("KhachHangId", khachHang.ID);
                HttpContext.Session.SetString("Role", "Customer");
                HttpContext.Session.SetString("AvatarUrl", khachHang.AnhDaiDien ?? "");

                // Khách hàng thì về Trang chủ (Home) chứ không vào Admin
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // 2. Kiểm tra Nhân viên / Admin trong database
            var nhanVien = _unitOfWork.NhanVienRepository.GetByLogin(email, password);
             if (nhanVien != null)
             {
                 HttpContext.Session.SetString("UserName", nhanVien.HoTen);
                 HttpContext.Session.SetString("Email", nhanVien.Email ?? "");
                 HttpContext.Session.SetString("Role", nhanVien.VaiTro); // "Admin" hoặc "Employee"
                 
                 // Chuyển hướng vào trang quản lý
                 return RedirectToRoute("AdminDonHang");
             }



            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string hoTen, string email, string password, string confirmPassword)
        {
            // Lưu lại giá trị form để hiển thị lại nếu có lỗi
            ViewBag.HoTen = hoTen;
            ViewBag.Email = email;
            ViewBag.ShowRegister = true;

            if (string.IsNullOrEmpty(hoTen) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.RegisterError = "Vui lòng điền đầy đủ thông tin đăng ký.";
                return View("Login");
            }
            if (password.Length < 6)
            {
                ViewBag.RegisterError = "Mật khẩu phải có tối thiểu 6 ký tự.";
                return View("Login");
            }

            // Kiểm tra ký tự đặc biệt trong Tên
            if (Regex.IsMatch(hoTen, @"[!@#$%^&*(),.?""':{}|<>]"))
            {
                ViewBag.RegisterError = "Họ tên không được chứa ký tự đặc biệt.";
                return View("Login");
            }
            if (password != confirmPassword)
            {
                ViewBag.RegisterError = "Mật khẩu xác nhận không khớp.";
                return View("Login");
            }

            // Kiểm tra email có đuôi @gmail.com
            if (!email.EndsWith("@gmail.com"))
            {
                ViewBag.RegisterError = "Email phải có đuôi @gmail.com";
                return View("Login");
            }

            // Kiểm tra email đã tồn tại chưa
            var existingByEmail = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            if (existingByEmail != null)
            {
                ViewBag.RegisterError = "Email này đã được sử dụng. Vui lòng chọn email khác!";
                return View("Login");
            }

            // Tạo tài khoản tự động từ email
            string taiKhoanTuDong = email.Split('@')[0];

            // Kiểm tra tài khoản đã tồn tại chưa
            var existingByTaiKhoan = await _unitOfWork.KhachHangRepository.GetByTaiKhoanAsync(taiKhoanTuDong);
            if (existingByTaiKhoan != null)
            {
                // Thêm số ngẫu nhiên nếu tài khoản đã tồn tại
                taiKhoanTuDong = taiKhoanTuDong + new Random().Next(100, 999).ToString();
            }

            // Tạo MaHienThi tự động
            string maHienThi = "KH" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99).ToString();

            try
            {
                var newKhach = new QuanLyVatTu_ASP.Areas.Admin.Models.KhachHang
                {
                    MaHienThi = maHienThi,
                    HoTen = hoTen,
                    Email = email,
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(password), // Mã hóa BCrypt
                    DiaChi = null, // Fix: Set to null to avoid MaxLength/Regex validation on empty string if any
                    SoDienThoai = null, // Fix: Set to null to pass Regex ^0\d{9}$ check
                    TaiKhoan = taiKhoanTuDong,
                    NgayTao = DateTime.Now,
                    DangNhapGoogle = false
                };

                _unitOfWork.KhachHangRepository.Add(newKhach);
                _unitOfWork.Save();

                ViewBag.Success = "Đăng ký thành công! Vui lòng đăng nhập.";
                ViewBag.ShowRegister = false;
                return View("Login");
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("UNIQUE KEY constraint"))
                {
                    ViewBag.RegisterError = "Email hoặc tài khoản đã được sử dụng. Vui lòng thử lại!";
                }
                else
                {
                    ViewBag.RegisterError = "Lỗi đăng ký: " + ex.Message;
                }

                return View("Login");
            }
        }

        public IActionResult Logout()
        {
            // Xóa toàn bộ Session
            HttpContext.Session.Clear();
            
            // Xóa Cookie Session (Dùng đúng tên đã config trong Program.cs)
            if (Request.Cookies[".QuanLyVatTu.Session"] != null)
            {
                Response.Cookies.Delete(".QuanLyVatTu.Session");
            }
            if (Request.Cookies[".AspNetCore.Session"] != null)
            {
                Response.Cookies.Delete(".AspNetCore.Session");
            }

            // Thêm Header để ngăn browser cache (Chống Back sau khi Logout)
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            
            return RedirectToAction("Login");
        }

        // ============= GOOGLE AUTHENTICATION =============
        
        /// <summary>
        /// Khởi tạo đăng nhập bằng Google
        /// </summary>
        [HttpGet]
        public IActionResult LoginByGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "Account")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Callback sau khi Google xác thực
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            if (!result.Succeeded)
            {
                ViewBag.Error = "Đăng nhập bằng Google thất bại.";
                return View("Login");
            }

            // Lấy thông tin từ Google claims
            var claims = result.Principal?.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Không thể lấy email từ tài khoản Google.";
                return View("Login");
            }

            // Kiểm tra xem email đã tồn tại trong DB chưa
            var existingUser = await _unitOfWork.KhachHangRepository.GetByEmailAsync(email);
            
            if (existingUser != null)
            {
                // Đăng nhập người dùng hiện tại
                HttpContext.Session.SetString("UserName", existingUser.HoTen);
                HttpContext.Session.SetString("Email", existingUser.Email ?? "");
                HttpContext.Session.SetInt32("KhachHangId", existingUser.ID);
                HttpContext.Session.SetString("Role", "Customer");
            }
            else
            {
                // Tạo tài khoản mới cho người dùng Google
                string taiKhoan = email.Split('@')[0];
                var existingByTaiKhoan = await _unitOfWork.KhachHangRepository.GetByTaiKhoanAsync(taiKhoan);
                if (existingByTaiKhoan != null)
                {
                    taiKhoan = taiKhoan + new Random().Next(100, 999).ToString();
                }

                var newUser = new KhachHang
                {
                    MaHienThi = "KH" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99).ToString(),
                    HoTen = name ?? email.Split('@')[0],
                    Email = email,
                    MatKhau = "", // Không cần mật khẩu cho Google login
                    DiaChi = null,
                    SoDienThoai = null,
                    TaiKhoan = taiKhoan,
                    NgayTao = DateTime.Now,
                    DangNhapGoogle = true
                };

                _unitOfWork.KhachHangRepository.Add(newUser);
                _unitOfWork.Save();

                HttpContext.Session.SetString("UserName", newUser.HoTen);
                HttpContext.Session.SetString("Email", newUser.Email);
                HttpContext.Session.SetInt32("KhachHangId", newUser.ID);
                HttpContext.Session.SetString("Role", "Customer");
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}