using Microsoft.AspNetCore.Mvc;
using QuanLyVatTu_ASP.Areas.Admin.ViewModels.NhanVien;
using QuanLyVatTu_ASP.Services.Interfaces;

namespace QuanLyVatTu_ASP.ViewComponents
{
    public class UserProfileHeaderViewComponent : ViewComponent
    {
        private readonly INhanVienService _nhanVienService;

        public UserProfileHeaderViewComponent(INhanVienService nhanVienService)
        {
            _nhanVienService = nhanVienService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var email = HttpContext.Session.GetString("Email");
            
            // Fallback info from session if DB lookup fails or is not needed for specific things
            var sessionName = HttpContext.Session.GetString("UserName");
            var sessionRole = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(email))
            {
                // If no email, check if we have session name/role at least (unlikely if authorized)
                 if (!string.IsNullOrEmpty(sessionName))
                 {
                     return View(new UserProfileViewModel
                     {
                         HoTen = sessionName,
                         VaiTro = sessionRole ?? "NhanVien",
                         Avatar = string.Empty // Để View hiển thị icon mặc định
                     });
                 }
                return Content(string.Empty);
            }

            var userProfile = await _nhanVienService.GetByEmailAsync(email);
            
            if (userProfile == null)
            {
                 // Fallback if user not found in DB but logged in via hardcode
                 return View(new UserProfileViewModel
                 {
                     HoTen = sessionName ?? "Unknown",
                     VaiTro = sessionRole ?? "NhanVien",
                     Avatar = string.Empty // Để View hiển thị icon mặc định
                 });
            }

            return View(userProfile);
        }
    }
}
