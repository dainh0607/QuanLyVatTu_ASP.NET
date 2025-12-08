using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Models.ViewModels;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Repositories.Interfaces;
using QuanLyVatTu_ASP.Repositories.Implementations;


namespace QuanLyVatTu_ASP.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IVatTuRepository _vatTuRepository;
        private readonly ILoaiVatTuRepository _loaiVatTuRepository;
        private readonly IKhachHangRepository _khachHangRepository;
        private readonly IDonHangRepository _donHangRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerController(
            IVatTuRepository vatTuRepository,
            ILoaiVatTuRepository loaiVatTuRepository,
            IKhachHangRepository khachHangRepository,
            IDonHangRepository donHangRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _vatTuRepository = vatTuRepository;
            _loaiVatTuRepository = loaiVatTuRepository;
            _khachHangRepository = khachHangRepository;
            _donHangRepository = donHangRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        // Trang chủ
        public IActionResult Index()
        {
            return View();
        }

        // Trang danh mục sản phẩm
        public IActionResult Categories()
        {
            return View();
        }

        // Trang chi tiết sản phẩm (cần tham số)
        public IActionResult ProductDetail(int id)
        {
            return View();
        }

        // Trang giỏ hàng
        public IActionResult Cart()
        {
            return View();
        }

        // Trang thanh toán
        public IActionResult Checkout()
        {
            return View();
        }

        // Trang danh sách yêu thích
        public IActionResult Wishlist()
        {
            return View();
        }

        // Trang giới thiệu
        public IActionResult About()
        {
            return View();
        }

        // Trang chính sách mua hàng
        public IActionResult PurchasePolicy()
        {
            return View();
        }

        // Trang tìm kiếm (thêm để xử lý form tìm kiếm)
        [HttpGet]
        public IActionResult Search(string query)
        {
            return View();
        }


        // API/Action để xử lý thêm vào giỏ hàng
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            return Json(new { success = true, message = "Đã thêm vào giỏ hàng" });
        }

        // API/Action để xử lý cập nhật giỏ hàng
        [HttpPost]
        public IActionResult UpdateCart(int cartItemId, int quantity)
        {
            return Json(new { success = true, message = "Đã cập nhật giỏ hàng" });
        }

        // API/Action để xử lý xóa khỏi giỏ hàng
        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            return Json(new { success = true, message = "Đã xóa khỏi giỏ hàng" });
        }

        //Profile
        // Thêm vào CustomerController.cs
        public async Task<IActionResult> Profile()
        {
            // Lấy ID khách hàng từ session (trong thực tế sẽ lấy từ authentication)
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");

            if (khachHangId == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                TempData["Message"] = "Vui lòng đăng nhập để xem thông tin cá nhân";
                return RedirectToAction("Login", "Account");
            }

            var khachHang = await _khachHangRepository.GetByIdAsync(khachHangId.Value);

            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng";
                return RedirectToAction("Index");
            }

            // Return a view with the customer data (assuming a view model is used)
            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var khachHang = await _khachHangRepository.GetByIdAsync(model.Id);

            if (khachHang == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khách hàng" });
            }

            // Kiểm tra mật khẩu cũ (trong thực tế cần mã hóa)
            if (khachHang.MatKhau != model.MatKhauCu) // Nên sử dụng hash password
            {
                return Json(new { success = false, message = "Mật khẩu cũ không chính xác" });
            }

            // Cập nhật mật khẩu mới (nên mã hóa)
            khachHang.MatKhau = model.MatKhauMoi;

            await _khachHangRepository.UpdateAsync(khachHang);

            return Json(new
            {
                success = true,
                message = "Đổi mật khẩu thành công"
            });
        }
           



    }
}