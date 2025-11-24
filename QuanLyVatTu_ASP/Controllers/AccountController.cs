using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Models;

namespace QuanLyVatTu_ASP.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Auth()
        {
            return View();
        }

    }
}
