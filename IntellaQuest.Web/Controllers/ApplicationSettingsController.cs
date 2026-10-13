using System;
using Microsoft.AspNetCore.Mvc;

namespace IntellaQuest.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ApplicationSettingsController : Controller
    {
        [HttpPost]
        public IActionResult GetApplicationContext()
        {
            var model = new
            {
                SiteKey = "",
                CurrentSiteDefaultCurrency = new { },
                CurrentSiteCurrencySymbol = new { },
                CurrentSiteISOCurrencySymbol = new { },
                ModuleType = "yeah",
                CurrentUser = new { },
                CurrentSite = new { },
                CurrentLanguage = new { },
                ActiveModules = new object[1],
                AvailableLanguages = new object[1] { new { ShortTitle = "US" } },
                ApplicationSettings = new { },
                SiteSettings = new { },
                ModuleSettings = new { },
                SystemTime = DateTime.UtcNow,
                CurrentYear = DateTime.Now.Year,
                AllApps = new { },
                OrderedApps = new { },
                CollaborationPortalUrl = ""
            };

            return Json(model);
        }
    }
}
