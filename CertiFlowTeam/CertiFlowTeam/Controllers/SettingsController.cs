using CertiFlowTeam.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CertiFlowTeam.Controllers
{
    public class SettingsController : BaseController
    {
        #region Dependencies

        private readonly DatabaseHelper _databaseHelper;

        public SettingsController(IConfiguration configuration)
        {
            _databaseHelper = new DatabaseHelper(configuration);
        }

        #endregion

        #region Page Actions

        public IActionResult Index()
        {
            return View();
        }

        #endregion

        #region Ajax Methods

        [HttpPost]
        public async Task<JsonResult> CheckDatabase()
        {
            try
            {
                var result = await _databaseHelper.CheckAndCreateTablesAsync();

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    details = result.Details
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message,
                    details = new List<string> { ex.ToString() }
                });
            }
        }

        #endregion
    }
}
