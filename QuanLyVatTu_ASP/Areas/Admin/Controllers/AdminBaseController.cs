using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Attributes;

namespace QuanLyVatTu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class AdminBaseController : Controller
    {
    }
}