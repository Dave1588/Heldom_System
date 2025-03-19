using Microsoft.AspNetCore.Mvc;

namespace Heldom_SYS.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult Cctv(int? index)
        {
            ViewData["InitialIndex"] = index ?? 0;
            return View();
        }
        public IActionResult log()
        {
            return View();
        }
        public IActionResult test()
        {
            return View();
        }

    }
}
