using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyVatTu_ASP.Attributes
{
    public class Authentication : ActionFilterAttribute
    {
        // Định nghĩa controller nào vai trò nào được truy cập
        private static readonly Dictionary<string, string[]> RolePermissions = new()
        {
            ["Quản trị"] = new[] { "DonHang", "HoaDon", "Voucher", "VatTu", "LoaiVatTu", "NhaCungCap", "KhachHang", "NhanVien", "ThongKe", "DanhGia" },
            ["Nhân viên"] = new[] { "DonHang", "Voucher", "KhachHang" },
            ["Kế toán"] = new[] { "HoaDon", "ThongKe" },
            ["Thủ kho"] = new[] { "VatTu", "LoaiVatTu", "NhaCungCap" },
        };

        // Trang mặc định cho từng vai trò khi bị từ chối quyền
        private static readonly Dictionary<string, string> DefaultController = new()
        {
            ["Quản trị"] = "DonHang",
            ["Nhân viên"] = "DonHang",
            ["Kế toán"] = "HoaDon",
            ["Thủ kho"] = "VatTu",
        };

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userName = context.HttpContext.Session.GetString("UserName");
            var role = context.HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userName))
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "Controller", "Account" },
                        { "Action", "Login" },
                        { "Area", "" }
                    });
                return;
            }

            var areaData = context.RouteData.DataTokens["area"] as string;

            if (areaData == "Admin")
            {
                // Customer không được vào Admin
                if (role == "Customer")
                {
                    context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "Controller", "Home" },
                        { "Action", "Index" },
                        { "Area", "" }
                    });
                    return;
                }

                // Kiểm tra phân quyền theo controller
                var controller = context.RouteData.Values["controller"]?.ToString();
                if (!string.IsNullOrEmpty(role) && !string.IsNullOrEmpty(controller)
                    && RolePermissions.ContainsKey(role))
                {
                    var allowedControllers = RolePermissions[role];
                    if (!allowedControllers.Contains(controller))
                    {
                        // Redirect về trang mặc định của vai trò
                        var defaultCtrl = DefaultController.GetValueOrDefault(role, "DonHang");
                        context.Result = new RedirectToRouteResult(
                            new RouteValueDictionary
                            {
                                { "Controller", defaultCtrl },
                                { "Action", "Index" },
                                { "Area", "Admin" }
                            });
                        return;
                    }
                }
            }
            base.OnActionExecuting(context);
        }
    }
}