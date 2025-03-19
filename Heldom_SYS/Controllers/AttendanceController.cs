using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Heldom_SYS.Controllers
{
    [Route("Attendance")]
    public class AttendanceController : Controller
    {
        private readonly SqlConnection DataBase;

        public AttendanceController(SqlConnection connection)
        {
            DataBase = connection;
        }

        [Route("Records")]
        public async Task<IActionResult> Records()
        {
            string? userId = HttpContext.Session.GetString("UserID");
            string? role = HttpContext.Session.GetString("Role");
            string? userName = HttpContext.Session.GetString("UserName");

            ViewBag.userId = userId;
            ViewBag.Role = role;
            ViewBag.userName = userName;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role) || role == "X")
            {
                return RedirectToAction("Index", "Login");
            }

            string sql = @"SELECT * FROM AttendanceRecord 
                           WHERE EmployeeID = @EmployeeID 
                           ORDER BY WorkDate DESC";
            var records = await DataBase.QueryAsync<AttendanceRecord>(sql, new { EmployeeID = userId });

            string leavesql = @"SELECT * FROM LeaveRecord 
                                WHERE EmployeeID = @EmployeeID 
                                ORDER BY StartTime DESC";
            var leaveRecords = await DataBase.QueryAsync<LeaveRecord>(leavesql, new { EmployeeID = userId });

            var viewModel = new AttendanceViewModel
            {
                AttendanceRecords = records,
                LeaveRecords = leaveRecords
            };

            return View(viewModel);
        }

        [Route("CheckIn")]
        [HttpPost]
        public async Task<IActionResult> CheckIn(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                return Json(new { success = false, message = "EmployeeId is required." });
            }

            string checkInSql = @"SELECT COUNT(*) FROM AttendanceRecord
                          WHERE EmployeeID = @EmployeeID AND WorkDate = @WorkDate";
            var existingCheckInCount = await DataBase.ExecuteScalarAsync<int>(checkInSql, new
            {
                EmployeeID = employeeId,
                WorkDate = DateTime.Now.Date
            });

            if (existingCheckInCount > 0)
            {
                return Json(new { success = false, message = "今天已簽到" });
            }

            string sql = "SELECT TOP 1 AttendanceID FROM AttendanceRecord ORDER BY AttendanceID DESC";
            var lastAttendanceId = await DataBase.QueryFirstOrDefaultAsync<string>(sql);
            string nextAttendanceId = GenerateNextAttendanceId(lastAttendanceId);

            var checkInTime = DateTime.Now;

            sql = @"
        INSERT INTO AttendanceRecord (AttendanceID, EmployeeID, WorkDate, CheckInTime)
        VALUES (@AttendanceID, @EmployeeID, @WorkDate, @CheckInTime)";

            var result = await DataBase.ExecuteAsync(sql, new
            {
                AttendanceID = nextAttendanceId,
                EmployeeID = employeeId,
                WorkDate = checkInTime.Date,
                CheckInTime = checkInTime
            });

            if (result > 0)
            {
                return Json(new { success = true, message = "簽到成功" });
            }

            return Json(new { success = false, message = "Failed to record attendance." });
        }

        [Route("CheckOut")]
        [HttpPost]
        public async Task<IActionResult> CheckOut(string attendanceId)
        {
            if (string.IsNullOrEmpty(attendanceId))
            {
                return Json(new { success = false, message = "AttendanceId is required." });
            }

            string checkInSql = @"SELECT CheckInTime FROM AttendanceRecord
                          WHERE AttendanceID = @AttendanceID AND CheckInTime IS NOT NULL";
            var checkInTime = await DataBase.QueryFirstOrDefaultAsync<DateTime?>(checkInSql, new
            {
                AttendanceID = attendanceId
            });

            if (!checkInTime.HasValue)
            {
                return Json(new { success = false, message = "請先簽到" });
            }

            string checkOutSql = @"SELECT CheckOutTime FROM AttendanceRecord
                           WHERE AttendanceID = @AttendanceID";
            var existingCheckOut = await DataBase.QueryFirstOrDefaultAsync<DateTime?>(checkOutSql, new
            {
                AttendanceID = attendanceId
            });

            if (existingCheckOut.HasValue)
            {
                return Json(new { success = false, message = "今天已簽退" });
            }

            string updateSql = @"UPDATE AttendanceRecord 
                         SET CheckOutTime = @CheckOutTime 
                         WHERE AttendanceID = @AttendanceID";

            var result = await DataBase.ExecuteAsync(updateSql, new
            {
                CheckOutTime = DateTime.Now,
                AttendanceID = attendanceId
            });

            if (result > 0)
            {
                return Json(new { success = true, message = "簽退成功" });
            }

            return Json(new { success = false, message = "Failed to record checkout." });
        }



        // 生成下一个 AttendanceID
        private string GenerateNextAttendanceId(string lastAttendanceId)
        {
            if (string.IsNullOrEmpty(lastAttendanceId))
            {
                return "A0001";
            }

            var numberPart = lastAttendanceId.Substring(1); 
            var nextNumber = int.Parse(numberPart) + 1; 

            return "A" + nextNumber.ToString("D4"); 
        }


        [Route("Info")]
        public IActionResult Info()
        {
            string? role = HttpContext.Session.GetString("Role");
           
            if (string.IsNullOrEmpty(role) || (role != "A" && role != "M"))
                {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Role = role;
            return View();
        }

        public class AttendanceViewModel
        {
            public IEnumerable<AttendanceRecord> AttendanceRecords { get; set; }
            public IEnumerable<LeaveRecord> LeaveRecords { get; set; }
        }
    }
}