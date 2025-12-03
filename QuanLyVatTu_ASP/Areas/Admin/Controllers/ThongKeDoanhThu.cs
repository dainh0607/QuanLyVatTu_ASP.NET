using Microsoft.AspNetCore.Mvc;

namespace QuanLyVatTu_ASP.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongKeDoanhThuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
