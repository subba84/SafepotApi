using Microsoft.AspNetCore.Mvc;

namespace Safepot.WebApp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
