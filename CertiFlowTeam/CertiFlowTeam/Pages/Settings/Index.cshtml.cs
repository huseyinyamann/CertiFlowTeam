using CertiFlowTeam.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CertiFlowTeam.Pages.Settings
{
    public class IndexModel : PageModel
    {
        #region Dependencies

        private readonly DatabaseHelper _databaseHelper;

        public IndexModel(IConfiguration configuration)
        {
            _databaseHelper = new DatabaseHelper(configuration);
        }

        #endregion

        #region Page Actions

        public void OnGet()
        {
        }

        #endregion

        #region Ajax Methods

        public async Task<JsonResult> OnPostCheckDatabaseAsync()
        {
            try
            {
                var result = await _databaseHelper.CheckAndCreateTablesAsync();

                return new JsonResult(new
                {
                    success = result.Success,
                    message = result.Message,
                    details = result.Details
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Bir hata oluştu: " + ex.Message,
                    details = new List<string> { ex.ToString() }
                });
            }
        }

        #endregion
    }
}
