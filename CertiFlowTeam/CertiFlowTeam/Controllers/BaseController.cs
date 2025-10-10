using CertiFlowTeam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace CertiFlowTeam.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!IsUserLoggedIn())
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }

            base.OnActionExecuting(context);
        }

        protected bool IsUserLoggedIn()
        {
            var userJson = HttpContext.Session.GetString("UserSession");
            return !string.IsNullOrEmpty(userJson);
        }

        protected UserSessionModel GetUserSession()
        {
            var userJson = HttpContext.Session.GetString("UserSession");

            if (string.IsNullOrEmpty(userJson))
            {
                return null;
            }

            return JsonSerializer.Deserialize<UserSessionModel>(userJson);
        }

        protected void SetUserSession(UserSessionModel userSession)
        {
            var userJson = JsonSerializer.Serialize(userSession);
            HttpContext.Session.SetString("UserSession", userJson);
        }
    }
}
