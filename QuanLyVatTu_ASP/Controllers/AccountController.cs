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
            // ---------------------------------------------------------
            // 1. KIỂM TRA BẢNG NHÂN VIÊN (Ưu tiên Admin/Nhân viên)
            // ---------------------------------------------------------
            // Lấy danh sách nhân viên và lọc (Giả sử Repository có hàm GetAll hoặc Get)
            // Lưu ý: Logic check Password hiện tại là text thuần (chưa mã hóa)
            var nhanVien = _unitOfWork.NhanVienRepository.GetAll()
                .FirstOrDefault(u => (u.Email == email || u.TaiKhoan == email) && u.MatKhau == password);

            if (nhanVien != null)
            {
                // Lưu thông tin cơ bản vào Session
                HttpContext.Session.SetString("UserName", nhanVien.HoTen);
                HttpContext.Session.SetString("Email", nhanVien.Email ?? "");

                // --- LOGIC PHÂN QUYỀN (MAPPING ROLE) ---
                // Chuyển đổi từ tiếng Việt trong DB sang Key tiếng Anh để code dễ xử lý
                string role = "Employee"; // Mặc định là nhân viên thường
                if (nhanVien.VaiTro == "Quản lý")
                {
                    role = "Admin";
                }

                // Lưu Role vào Session để Attribute [Authentication] kiểm tra sau này
                HttpContext.Session.SetString("Role", role);

                // Chuyển hướng vào khu vực Admin
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            // ---------------------------------------------------------
            // 2. KIỂM TRA BẢNG KHÁCH HÀNG (Nếu không phải nhân viên)
            // ---------------------------------------------------------
            var khachHang = _unitOfWork.KhachHangRepository.GetAll()
                .FirstOrDefault(u => (u.Email == email || u.TaiKhoan == email) && u.MatKhau == password);

            if (khachHang != null)
            {
                // Lưu Session cho khách hàng
                HttpContext.Session.SetString("UserName", khachHang.HoTen);
                HttpContext.Session.SetString("Role", "Customer");

                // Chuyển hướng ra trang chủ (Layout Khách hàng)
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // ---------------------------------------------------------
            // 3. ĐĂNG NHẬP THẤT BẠI
            // ---------------------------------------------------------
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