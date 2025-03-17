using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Heldom_SYS.Controllers
{
    public class ProfileController : Controller
    {
        private readonly SqlConnection DataBase;
        private readonly IUserStoreService UserRoleStore;
        private readonly ConstructionDbContext DbContext;
        public ProfileController(SqlConnection connection, IUserStoreService _UserRoleStore, ConstructionDbContext dbContext)
        {
            DataBase = connection;
            UserRoleStore = _UserRoleStore;
            DbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeWithDetail()
        {

            string userID = UserRoleStore.UserID;
            var employeeWithDetail = await DbContext.Employees
                .Where(employee => employee.EmployeeId == userID)
                .Join(DbContext.EmployeeDetails,
                employee => employee.EmployeeId,
                detail => detail.EmployeeId,
                (employee, detail) => new
                {
                    photo = Convert.ToBase64String(detail.EmployeePhoto),
                    name = detail.EmployeeName,
                    email = detail.Mail,
                    yearsbetween = (int)((employee.ResignationDate ?? DateTime.Now) - employee.HireDate).TotalDays / 365,
                    leaveDays = detail.AnnualLeave,
                    hireDate = employee.HireDate,
                    position = employee.Position,
                    department = detail.Department,
                    employeeID = employee.EmployeeId,
                    birthDate = detail.BirthDate,
                    phone = detail.PhoneNumber,
                    address = detail.Address,
                    emerContact = detail.EmergencyContact,
                    emerConPhone = detail.EmergencyContactPhone
                }).ToListAsync();
            return Ok(employeeWithDetail);
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
