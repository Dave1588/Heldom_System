using Heldom_SYS.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text.Json;

namespace Heldom_SYS.Controllers
{
    public class ProjectController : Controller
    {
        public class UsePrintCategary
        {
            public string categaryID { get; set; }
            public string categaryName { get; set; }
        }
        private readonly IPrintCategoryService PrintCategory;
        //紀錄PrintCategory種類 與 當前人員UserRoleStore
        private readonly IUserStoreService UserRoleStore;
        public ProjectController(IPrintCategoryService _PrintCategory,IUserStoreService _UserRoleStore)
        {
            PrintCategory = _PrintCategory;
            UserRoleStore = _UserRoleStore;
        }

        public IActionResult Issues(string type, string id, string motion)
        {
            //UserRoleStore.SetRole("A");
            ViewBag.Role = UserRoleStore.GetRole();

            //if ((ViewBag.Role == "X") || (ViewBag.Role == "P"))
            //{
            //    return RedirectToAction("Index", "Login");
            //}


            if (type.IsNullOrEmpty() || id.IsNullOrEmpty() || motion.IsNullOrEmpty()) {
                return View("Issues");
            }
            else
            {
                ViewBag.Type = type;
                ViewBag.ID = id;
                ViewBag.Motion = motion;
                return View("IssuesDetail");
            }

        }

        public IActionResult BlueprintsCategories()
        {

            ViewBag.PrintOner = UserRoleStore.UserID;
            UsePrintCategary usePrintCategary = new UsePrintCategary { categaryID="PC2", categaryName= "建築設計圖" };
            HttpContext.Session.SetString("UsePrintCategary", JsonSerializer.Serialize(usePrintCategary));
            return View();
        }

        public IActionResult Blueprints()
        {
            //ViewBag.categaryTest2 = categaryTest;
            var categaryJson = HttpContext.Session.GetString("UsePrintCategary");
            UsePrintCategary useCategary = JsonSerializer.Deserialize<UsePrintCategary>(categaryJson);
            ViewBag.cID = useCategary.categaryID;
            ViewBag.cName=useCategary.categaryName;
            return View();
        }

        public IActionResult BlueprintsDetail()
        {
            return View();
        }

        public IActionResult BlueprintsEdit()
        {
            return View();
        }



    }
}
