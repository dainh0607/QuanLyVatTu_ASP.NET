using Microsoft.AspNetCore.Mvc;

namespace QuanLyVatTu_ASP.Models
{
    public class Customer : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
