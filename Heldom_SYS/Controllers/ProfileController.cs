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

        [Route("/Profile/Settings")]
        public IActionResult Settings()
        {
            return View();
        }


        [Route("/Profile/Account")]
        public IActionResult Account()
        {
            ViewBag.role = UserRoleStore.Role;
            return View();
        }

        [Route("/Profile/NewAccount")]
        public IActionResult NewAccount()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetIndexData()
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
                    phoneNumber = detail.PhoneNumber,
                    address = detail.Address,
                    emergencyContact = detail.EmergencyContact,
                    emergencyContactPhone = detail.EmergencyContactPhone
                }).ToListAsync();
            return Ok(employeeWithDetail);
        }

        [HttpGet]
        public async Task<IActionResult> GetSettingsData()
        {
            try
            {
                string userID = UserRoleStore.UserID;

                var employeeWithDetail = await DbContext.Employees
                    .Where(employee => employee.EmployeeId == userID)
                    .Join(DbContext.EmployeeDetails,
                    employee => employee.EmployeeId,
                    detail => detail.EmployeeId,
                    (employee, detail) => new
                    {
                        name = detail.EmployeeName,
                        position = employee.Position,
                        employeeID = employee.EmployeeId,
                        birthDate = detail.BirthDate,
                        phoneNumber = detail.PhoneNumber,
                        address = detail.Address,
                        emergencyContact = detail.EmergencyContact,
                        emergencyContactPhone = detail.EmergencyContactPhone
                    }).ToListAsync();
                return Ok(employeeWithDetail);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "資料取得失敗", error = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployeeInfo([FromBody] EmployeeDetailUpdateModel model)
        {
            try
            {
                string userID = UserRoleStore.UserID;

                // 更新 EmployeeDetail 表
                var employeeDetail = await DbContext.EmployeeDetails
                    .FirstOrDefaultAsync(ed => ed.EmployeeId == userID);

                if (employeeDetail != null)
                {
                    employeeDetail.PhoneNumber = model.phoneNumber;
                    employeeDetail.Address = model.address;
                    employeeDetail.EmergencyContact = model.emergencyContact;
                    employeeDetail.EmergencyContactPhone = model.emergencyContactPhone;

                    await DbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "資料更新成功" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "找不到員工資料" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "更新失敗：" + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountData()
        {
            try
            {
                string userID = UserRoleStore.UserID;

                var employeesWithDetail = await DbContext.Employees
                    .Where(employee => employee.EmployeeId == userID)
                    .Join(DbContext.EmployeeDetails,
                    employee => employee.EmployeeId,
                    detail => detail.EmployeeId,
                    (employee, detail) => new
                    {
                        photo = Convert.ToBase64String(detail.EmployeePhoto),
                        name = detail.EmployeeName,
                        employeeID = employee.EmployeeId,
                        phoneNumber = detail.PhoneNumber,
                        department = detail.Department,
                        position = employee.Position,
                        hireDate = employee.HireDate,
                        activestat = employee.IsActive

                    }).ToListAsync();
                return Ok(employeesWithDetail);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "資料取得失敗", error = ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployeeDetail([FromBody] EmployeeDetailCreateModel model)
        {
            try
            {
                string userID = UserRoleStore.UserID;

                // 更新 EmployeeDetail 表
                var employeeDetail = await DbContext.EmployeeDetails
                    .FirstOrDefaultAsync(ed => ed.EmployeeId == userID);

                if (employeeDetail != null)
                {
                    //employeeDetail.PhoneNumber = model.phoneNumber;
                    //employeeDetail.Address = model.address;
                    //employeeDetail.EmergencyContact = model.emergencyContact;
                    //employeeDetail.EmergencyContactPhone = model.emergencyContactPhone;

                    await DbContext.SaveChangesAsync();

                    return Ok(new { success = true, message = "資料更新成功" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "找不到員工資料" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "更新失敗：" + ex.Message });
            }
        }
    }

    public class EmployeeDetailUpdateModel
    {
        public string phoneNumber { get; set; }
        public string address { get; set; }
        public string emergencyContact { get; set; }
        public string emergencyContactPhone { get; set; }
    }

    public class EmployeeDetailCreateModel
    {

    }

}
