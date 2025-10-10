using Microsoft.AspNetCore.Mvc;

namespace CertiFlowTeam.Controllers
{
    public class HomeController : BaseController
    {
        #region Page Actions

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        #endregion
    }
}
