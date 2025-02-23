using Microsoft.AspNetCore.Mvc;

namespace Heldom_SYS.Controllers
{
    // https://localhost:7142/Attendance/Records
    public class AttendanceController : Controller
    {
        public IActionResult Records()
        {
            return View();
        }

        public IActionResult Info()
        {
            return View();
        }
    }
}
