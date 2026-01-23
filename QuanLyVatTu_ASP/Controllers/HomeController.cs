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
    }
}