using Microsoft.AspNetCore.Mvc;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ChiTietDonHangController : Controller
    {
        public IActionResult Index(int donHangId)
        {
            ViewBag.DonHangId = donHangId;
            return View();
        }

        public IActionResult Create(int donHangId)
        {
            ViewBag.DonHangId = donHangId;
            return View();
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Id = id;
            return View();
        }
    }
}
