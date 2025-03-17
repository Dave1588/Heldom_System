using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;

namespace Heldom_SYS.Controllers
{
    public class LoginController : Controller
    {
        private readonly SqlConnection DataBase;
        private readonly IUserStoreService UserRoleStore;
        public LoginController(SqlConnection connection, IUserStoreService _UserRoleStore)
        {
            DataBase = connection;
            UserRoleStore = _UserRoleStore;
        }

        public IActionResult Index()
        {
            return View();
        }

        public class LogInData
        {
            public required string Type { get; set; }
            public required string Account { get; set; }
            public required string PassWord { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Enter([FromBody] LogInData data)
        {
            UserRoleStore.UserID = "";
            UserRoleStore.UserName = "";
            UserRoleStore.SetRole("X");
            //await DataBase.OpenAsync();

            if (data.Type == "visitor") {
                
                string sql = @"SELECT * FROM Temporarier
                    where EmployeeName = @EmployeeName and PhoneNumber = @PhoneNumber"
                ;

                //await _DataBase.QueryAsync<type>(sql);
                Temporarier? user = await DataBase.QueryFirstOrDefaultAsync<Temporarier>(sql, 
                    new { 
                        EmployeeName = data.Account,
                        PhoneNumber = data.PassWord,
                    });
                
                if (user != null) {
                    UserRoleStore.UserID = user.EmployeeId;
                    UserRoleStore.UserName= user.EmployeeName;
                }

            }
            else if(data.Type == "employee")
            {
                string sql = @"SELECT * FROM EmployeeDetail
                    where EmployeeName = @EmployeeName and Password = @Password"
                ;

                EmployeeDetail? user = await DataBase.QueryFirstOrDefaultAsync<EmployeeDetail>(sql,
                    new
                    {
                        EmployeeName = data.Account,
                        Password = data.PassWord,
                    });

                if (user != null)
                {
                    UserRoleStore.UserID = user.EmployeeId;
                    UserRoleStore.UserName = user.EmployeeName;
                }
            }

            if (!UserRoleStore.UserID.IsNullOrEmpty())
            {
                string roleSql = @"SELECT * FROM Employee where EmployeeID = @EmployeeID";
                Employee? role = await DataBase.QueryFirstOrDefaultAsync<Employee>(roleSql,
                    new
                    {
                        EmployeeID = UserRoleStore.UserID,
                    });

                if (role != null) {
                    string roleName = role.PositionRole;
                    UserRoleStore.SetRole(roleName);
                }
                
            }

            string url = "";

            if (UserRoleStore.Role == "A" || UserRoleStore.Role == "M") {
                url = "Dashboard/Dashboard";
            }
            else if (UserRoleStore.Role == "E" || UserRoleStore.Role == "P"){
                url = "Attendance/Records";
            }

            var response = new
            {
                route = url
            };

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");

        }

    }
}
