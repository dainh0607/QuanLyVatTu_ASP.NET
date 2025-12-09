using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Attributes;

namespace QuanLyVatTu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authentication]
    public class AdminBaseController : Controller
    {
    }
}