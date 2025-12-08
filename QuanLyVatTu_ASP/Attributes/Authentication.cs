using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyVatTu.Attributes
{
    public class Authentication : ActionFilterAttribute
    {
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
                        { "Action", "Auth" }
                    });
                return;
            }

            var areaData = context.RouteData.DataTokens["area"] as string;

            if (areaData == "Admin")
            {
                if (role == "Customer")
                {
                    context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "Controller", "Home" },
                        { "Action", "Index" },
                        { "Area", "" }
                    });
                }
            }
        }
    }
}