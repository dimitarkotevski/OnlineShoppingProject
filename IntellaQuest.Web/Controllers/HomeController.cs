using Microsoft.AspNetCore.Mvc;

namespace IntellaQuest.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Home()
        {
            return View();
        }
    }
}
