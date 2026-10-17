using Microsoft.AspNetCore.Mvc;

namespace NexCart.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Home()
        {
            return View();
        }
    }
}
