using Microsoft.AspNetCore.Mvc;

namespace TradingApp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
