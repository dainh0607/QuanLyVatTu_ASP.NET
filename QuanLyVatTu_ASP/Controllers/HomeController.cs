using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Models;

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
