using Microsoft.AspNetCore.Mvc;

namespace Heldom_SYS.Controllers
{
    public class ProfileController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        public IActionResult Account()
        {
            return View();
        }
        public IActionResult NewAccount()
        {
            return View();
        }


    }
}
