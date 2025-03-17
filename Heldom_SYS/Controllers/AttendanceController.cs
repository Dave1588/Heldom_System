using Heldom_SYS.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Heldom_SYS.Controllers
{
    
    [Route("Attendance")]
    public class AttendanceController : Controller
    {
        private readonly IUserStoreService UserRoleStore;

        public AttendanceController(IUserStoreService _UserRoleStore)
        {
            UserRoleStore = _UserRoleStore;
        }

        [Route("Records")]
        public IActionResult Records()
        {
            ViewBag.Role = UserRoleStore.GetRole();

            //if (ViewBag.Role == "X") {
            //    return RedirectToAction("Index","Login");
            //}

            return View();
        }

        [Route("Info")]
        public IActionResult Info()
        {
            ViewBag.Role = UserRoleStore.GetRole();

            //if ((ViewBag.Role == "A") || (ViewBag.Role == "M"))
            //{
            //    return View();
            //}
            //else
            //{
            //    return RedirectToAction("Index", "Login");
            //}

            return View();

        }
    }
}
