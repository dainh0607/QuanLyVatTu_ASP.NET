using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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
