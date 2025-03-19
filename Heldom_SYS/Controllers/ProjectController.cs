using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;


namespace Heldom_SYS.Controllers
{
    public class ProjectController : Controller
    {
        //BP
        private ConstructionDbContext db ;
        private readonly SqlConnection DataBase;
        public class UsePrintCategary
        {
            public required string  categaryID { get; set; }
            public required string categaryName { get; set; }
        }
 
        //紀錄當前人員UserRoleStore
        private readonly IUserStoreService UserRoleStore;
        public ProjectController(SqlConnection connection,ConstructionDbContext _db,IUserStoreService _UserRoleStore)
        {
            DataBase = connection;
            db = _db;
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
        //[HttpGet] /Project/Myfunction
        //public async Task<IActionResult> Myfunction(data)
        //{
        //    new { 
        //    name = 123,
        //    id = 456
        //    }
        //    var bee = Convert.deserialize(data)
        //        bee.name
        //    var apple = await DataBase.QueryFirstAsync();
        //    return Ok(apple);
        //}
        public IActionResult BlueprintsCategories()
        {
            //連線資料庫取categary
            string sql = @"SELECT * FROM PrintCategory";
            IEnumerable<dynamic>? PrintCategories =  DataBase.Query<dynamic>(sql);
            PrintCategories = PrintCategories.Select(p => new {
                p.PrintCategoryID,
                p.PrintCategory
            }).ToList();
            foreach (var item in PrintCategories)
            {
                Console.WriteLine((string)item.PrintCategoryID);
                Console.WriteLine((string)item.PrintCategory);
                string pcategary = (string)item.PrintCategory;
                string pcategaryID = item.PrintCategoryID.ToString();
                Console.WriteLine(item.PrintCategoryID.GetType());
                //HttpContext.Session.SetString($"{pcategaryID}", JsonConvert.SerializeObject(pcategary));
                HttpContext.Session.SetString(pcategaryID, JsonConvert.SerializeObject(pcategary));
            }

            //userName
            TempData["PrintOner"] = UserRoleStore.UserName;

            return View();
        }

            public IActionResult Blueprints(string id)
        {
            Console.WriteLine($"傳入 id: {id}");
            Console.WriteLine($"Session 內的 Keys: {string.Join(", ", HttpContext.Session.Keys)}");


            //  從 Session 取出並解析 JSON
            // 確保 id 不是 null 或空字串
            if (!string.IsNullOrEmpty(id))
            {
                // 嘗試從 Session 取出值
                var jsonString = HttpContext.Session.GetString(id);

                if (!string.IsNullOrEmpty(jsonString))
                {
                    // 解析 JSON
                    string? categories = JsonConvert.DeserializeObject<string>(jsonString);
                    TempData["categary"] = categories;
                    TempData["ID"] = id;
                }
                else
                {
                    // Session 內沒有該 id 的資料
                    Console.WriteLine($"Session 沒有找到 key: {id}");
                }
            }
            else
            {
                Console.WriteLine("id 不能為 null 或空字串");
            }
            //使用者當前藍圖種類usedCategary
            HttpContext.Session.SetString("usedCategary", JsonConvert.SerializeObject(id));

            //顯示藍圖實體
            string sql= @"SELECT  BlueprintName,EmployeeID, BlueprintVersion, 
            BlueprintFile, UploadTime FROM Blueprint 
            where [PrintCategoryID] = @id and PrintStatus = 1";

            IEnumerable<Blueprint> blueprints = DataBase.Query<Blueprint>(sql, new { id });
            var prints = blueprints.Select(p => new
            {
                Name = p.BlueprintName,
                Manager = p.EmployeeId,
                Version = p.BlueprintVersion,
                UploadDate = p.UploadTime,
                Image = p.BlueprintFile
            }).ToList();

            return View(prints);
        }

        public IActionResult BlueprintsDetail()
        {
            return View();
        }
        //[HttpGet("Project/Myfunction")] 
        //public async Task<IActionResult> Myfunction([FromQuery] string data)
        //{
        //    //var bee = System.Text.Json.JsonSerializer.Deserialize<dynamic>(data);
        //    var bee = JsonConvert.DeserializeObject<UsingCategary>(data);
        //    var name = bee?.name;
        //    var id = bee?.id;
        //    //bee.name
        //    //var apple = await DataBase.QueryFirstAsync();


        //    //return Ok(apple);
        //    return (bee);
        //}
        //public class UsingCategary
        //{
        //    public string Name { get; set; }
        //    public string Id { get; set; }
        //}
        [HttpGet("Project/GetUsingCategary")]
        public string GetUsingCategary()
        {
            var jsonString = HttpContext.Session.GetString("usedCategary");
            string? categories="";

            if (!string.IsNullOrEmpty(jsonString))
            {
                 categories = JsonConvert.DeserializeObject<string>(jsonString);
                HttpContext.Session.SetString("usedCategary", jsonString);
            }


            return categories??"";
            //?? =>前面存在的話 = 後面
        }
        public IActionResult BlueprintsEdit()
        {
            //var jsonString = HttpContext.Session.GetString("usedCategary");

            //if (!string.IsNullOrEmpty(jsonString))
            //{
            //    string? categories = JsonConvert.DeserializeObject<string>(jsonString);
            //    TempData["categaryID"] = categories;
            //}
            return View();
        }



    }
}
