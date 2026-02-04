using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Repositories;
using QuanLyVatTu_ASP.Repositories.Interfaces;

namespace QuanLyVatTu_ASP.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _unitOfWork.VatTuRepository.GetAllAsync();
            var featuredProducts = products.OrderByDescending(p => p.ID).Take(8).ToList();
            // var featuredProducts = new List<QuanLyVatTu_ASP.Areas.Admin.Models.VatTu>();
            return View(featuredProducts);
        }
        public async Task<IActionResult> Contact()
        {
            var khachHangId = HttpContext.Session.GetInt32("KhachHangId");
            if (khachHangId != null)
            {
                var khachHang = await _unitOfWork.KhachHangRepository.GetByIdAsync(khachHangId.Value);
                return View(khachHang);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string fullName, string email, string phone, string content)
        {
            // Xử lý gửi mail hoặc lưu database ở đây
            // Tạm thời hiển thị thông báo thành công
            TempData["Success"] = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi sớm nhất!";
            return RedirectToAction("Contact");
        }
    }
}