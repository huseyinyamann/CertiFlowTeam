using CertiFlowTeam.Helpers;
using CertiFlowTeam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CertiFlowTeam.Controllers
{
    public class AuthController : BaseController
    {
        #region Dependencies

        private readonly IConfiguration _configuration;
        private readonly CompanyHelper _companyHelper;
        private readonly UserHelper _userHelper;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            _companyHelper = new CompanyHelper(configuration);
            _userHelper = new UserHelper(configuration);
        }

        #endregion

        #region Override Methods

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var actionName = context.ActionDescriptor.RouteValues["action"];

            if (actionName == "Logout")
            {
                base.OnActionExecuting(context);
                return;
            }

            if (actionName != "CheckSession" && IsUserLoggedIn())
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }

        #endregion

        #region Page Actions

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        #endregion

        #region Ajax Methods

        [HttpPost]
        public async Task<JsonResult> DoLogin(FormCollection form)
        {
            var model = new UserLoginModel
            {
                Email = form["email"],
                Password = form["password"],
                RememberMe = form["rememberMe"] == "true"
            };

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return Json(new { success = false, message = "Email ve şifre alanları zorunludur" });
            }

            var result = await _userHelper.LoginAsync(model);

            if (result.Success && result.Data != null)
            {
                SetUserSession(result.Data);
                return Json(new { success = true, message = result.Message, redirectUrl = Url.Action("Index", "Home") });
            }

            return Json(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<JsonResult> DoRegister(FormCollection form)
        {
            var companyModel = new CompanyRegisterModel
            {
                CompanyName = form["companyName"],
                TaxNumber = form["taxNumber"],
                Address = form["address"],
                Phone = form["companyPhone"],
                Email = form["companyEmail"],
                AuthorizedPerson = form["authorizedPerson"],
                AuthorizedPhone = form["authorizedPhone"]
            };

            var userModel = new UserRegisterModel
            {
                FullName = form["fullName"],
                Email = form["userEmail"],
                Password = form["password"],
                PasswordConfirm = form["passwordConfirm"],
                Phone = form["userPhone"]
            };

            if (string.IsNullOrWhiteSpace(companyModel.CompanyName))
            {
                return Json(new { success = false, message = "Firma adı zorunludur" });
            }

            if (string.IsNullOrWhiteSpace(companyModel.TaxNumber))
            {
                return Json(new { success = false, message = "Vergi numarası zorunludur" });
            }

            if (string.IsNullOrWhiteSpace(companyModel.Email))
            {
                return Json(new { success = false, message = "Firma email adresi zorunludur" });
            }

            if (string.IsNullOrWhiteSpace(userModel.FullName))
            {
                return Json(new { success = false, message = "Ad Soyad zorunludur" });
            }

            if (string.IsNullOrWhiteSpace(userModel.Email))
            {
                return Json(new { success = false, message = "Email adresi zorunludur" });
            }

            if (string.IsNullOrWhiteSpace(userModel.Password))
            {
                return Json(new { success = false, message = "Şifre zorunludur" });
            }

            if (userModel.Password != userModel.PasswordConfirm)
            {
                return Json(new { success = false, message = "Şifreler eşleşmiyor" });
            }

            if (userModel.Password.Length < 6)
            {
                return Json(new { success = false, message = "Şifre en az 6 karakter olmalıdır" });
            }

            var companyResult = await _companyHelper.CreateCompanyAsync(companyModel);

            if (!companyResult.Success)
            {
                return Json(new { success = false, message = companyResult.Message });
            }

            userModel.CompanyId = companyResult.Data;

            var userResult = await _userHelper.CreateUserAsync(userModel);

            if (!userResult.Success)
            {
                return Json(new { success = false, message = userResult.Message });
            }

            return Json(new
            {
                success = true,
                message = "Kayıt başarılı! Giriş sayfasına yönlendiriliyorsunuz...",
                redirectUrl = Url.Action("Login", "Auth")
            });
        }

        [HttpPost]
        public JsonResult CheckSession()
        {
            if (IsUserLoggedIn())
            {
                var userSession = GetUserSession();
                return Json(new { success = true, isLoggedIn = true, user = userSession });
            }

            return Json(new { success = true, isLoggedIn = false });
        }

        #endregion
    }
}
