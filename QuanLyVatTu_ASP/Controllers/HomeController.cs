using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyVatTu_ASP.DTO.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}
