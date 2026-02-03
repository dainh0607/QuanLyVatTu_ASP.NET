using Microsoft.AspNetCore.Mvc;

namespace QuanLyVatTu_ASP.Controllers
{
    public class ChinhSachController : Controller
    {
        /// <summary>
        /// Trang tổng hợp các chính sách
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Chính sách thanh toán (quy định cọc, biến động giá)
        /// </summary>
        public IActionResult ThanhToan()
        {
            return View();
        }

        /// <summary>
        /// Chính sách giao hàng & vận chuyển
        /// </summary>
        public IActionResult GiaoHang()
        {
            return View();
        }

        /// <summary>
        /// Chính sách đổi trả & hoàn tiền
        /// </summary>
        public IActionResult DoiTra()
        {
            return View();
        }

        /// <summary>
        /// Chính sách bảo hành
        /// </summary>
        public IActionResult BaoHanh()
        {
            return View();
        }
    }
}
