using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Heldom_SYS.Controllers
{
    [Route("Attendance")]
    public class AttendanceController : Controller
    {
        private readonly SqlConnection DataBase;
        private readonly ConstructionDbContext _context;

        public AttendanceController(SqlConnection connection, ConstructionDbContext context)
        {
            DataBase = connection;
            _context = context;
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

            string annualLeaveSql = @"SELECT AnnualLeave FROM EmployeeDetail WHERE EmployeeId = @EmployeeId";
            var annualLeave = await DataBase.QueryFirstOrDefaultAsync<byte>(annualLeaveSql, new { EmployeeId = userId });

            string usedLeaveSql = @"SELECT SUM(SpentHours) FROM LeaveRecord 
                            WHERE EmployeeID = @EmployeeID AND LeaveStatus = 1"; // 審核
            var usedHours = await DataBase.QueryFirstOrDefaultAsync<int?>(usedLeaveSql, new { EmployeeID = userId }) ?? 0;

            int remainingLeave = annualLeave - usedHours;
            ViewBag.RemainingLeave = remainingLeave >= 0 ? remainingLeave : 0; 

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
                return Json(new { success = false, message = "AttendanceId 是必須的。" });
            }

            // 檢查該 AttendanceId 是否存在且屬於當天
            string checkSql = @"SELECT CheckInTime, WorkDate FROM AttendanceRecord
                        WHERE AttendanceID = @AttendanceID AND CheckInTime IS NOT NULL";
            var record = await DataBase.QueryFirstOrDefaultAsync<dynamic>(checkSql, new
            {
                AttendanceID = attendanceId
            });

            if (record == null)
            {
                return Json(new { success = false, message = "請先簽到或記錄不存在" });
            }

            DateTime checkInTime = record.CheckInTime;
            DateTime workDate = record.WorkDate;
            DateTime today = DateTime.Now.Date;

            // 確保簽退的記錄是當天的
            if (workDate != today)
            {
                return Json(new { success = false, message = "只能簽退當天的出勤記錄,請聯絡直屬主管" });
            }

            // 檢查是否已經簽退
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

            // 更新簽退時間
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

            return Json(new { success = false, message = "簽退失敗" });
        }


        // 生成下一个 AttendanceID
        private string GenerateNextAttendanceId(string? lastAttendanceId)
        {
            if (string.IsNullOrEmpty(lastAttendanceId))
            {
                return "A0001";
            }

            var numberPart = lastAttendanceId.Substring(1); 
            var nextNumber = int.Parse(numberPart) + 1; 

            return "A" + nextNumber.ToString("D4"); 
        }

        [Route("LeaveRequest")]
        [HttpPost]
        public async Task<IActionResult> LeaveRequest(string employeeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (string.IsNullOrEmpty(employeeId))
                {
                    return Json(new { success = false, message = "EmployeeId 是必須的。" });
                }

                if (startDate >= endDate)
                {
                    return Json(new { success = false, message = "結束時間必須晚於開始時間" });
                }

                // 檢查時間重疊
                string checkSql = @"
            SELECT COUNT(*) FROM LeaveRecord 
            WHERE EmployeeID = @EmployeeID 
            AND (@StartDate BETWEEN StartTime AND EndTime 
                 OR @EndDate BETWEEN StartTime AND EndTime 
                 OR StartTime BETWEEN @StartDate AND @EndDate)";
                var overlapCount = await DataBase.ExecuteScalarAsync<int>(checkSql, new
                {
                    EmployeeID = employeeId,
                    StartDate = startDate,
                    EndDate = endDate
                });

                if (overlapCount > 0)
                {
                    return Json(new { success = false, message = "該時間段已有請假記錄" });
                }

                // 計算請假時數
                double totalHours = CalculateLeaveHours(startDate, endDate);

                // 確保請假時數不超過 255 小時
                if (totalHours > 255)
                {
                    return Json(new { success = false, message = "請假時數超過上限（255小時）" });
                }

                // 插入請假記錄
                string sql = @"
            INSERT INTO LeaveRecord (EmployeeID, StartTime, EndTime, SpentHours, LeaveStatus)
            VALUES (@EmployeeID, @StartTime, @EndTime, @SpentHours, @LeaveStatus)";

                var result = await DataBase.ExecuteAsync(sql, new
                {
                    EmployeeID = employeeId,
                    StartTime = startDate,
                    EndTime = endDate,
                    SpentHours = (byte)totalHours,
                    LeaveStatus = false // 未批准
                });

                if (result > 0)
                {
                    return Json(new { success = true, message = "請假申請成功" });
                }

                return Json(new { success = false, message = "請假申請失敗" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"請假申請錯誤: {ex.Message}");
                return Json(new { success = false, message = $"伺服器錯誤: {ex.Message}" });
            }
        }
        //請假時數判斷
        private double CalculateLeaveHours(DateTime startDate, DateTime endDate)
        {
            double totalHours = 0;
            DateTime currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                DateTime workStart = currentDate.AddHours(8); 
                DateTime lunchStart = currentDate.AddHours(12);
                DateTime lunchEnd = currentDate.AddHours(13);  
                DateTime workEnd = currentDate.AddHours(17);   

                
                DateTime dayStart = (currentDate == startDate.Date) ? startDate : workStart;
                DateTime dayEnd = (currentDate == endDate.Date) ? endDate : workEnd;

                // 確保請假時間在上班時段內
                if (dayStart < workStart) dayStart = workStart;
                if (dayEnd > workEnd) dayEnd = workEnd;
                if (dayStart >= dayEnd) continue; 

                
                double hours = (dayEnd - dayStart).TotalHours;

                // 若請假時間橫跨午休，需扣除 1 小時
                if (dayStart < lunchStart && dayEnd > lunchEnd)
                {
                    hours -= 1;
                }
                else if (dayStart < lunchStart && dayEnd > lunchStart && dayEnd <= lunchEnd)
                {
                    hours -= (dayEnd - lunchStart).TotalHours;
                }
                else if (dayStart >= lunchStart && dayStart < lunchEnd)
                {
                    hours -= (lunchEnd - dayStart).TotalHours;
                }

                totalHours += Math.Max(0, hours);
                currentDate = currentDate.AddDays(1);
            }

            return totalHours;
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

        [Route("GetAttendanceRecords")]
        [HttpGet]
        public async Task<IActionResult> GetAttendanceRecords(string employeeName, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.AttendanceRecords
                .Include(ar => ar.Employee)
                .ThenInclude(e => e.EmployeeDetail)
                .AsQueryable();

            if (!string.IsNullOrEmpty(employeeName))
            {
                query = query.Where(ar => ar.Employee != null && ar.Employee.EmployeeDetail != null
                    && ar.Employee.EmployeeDetail.EmployeeName.Contains(employeeName));
            }
            if (startDate.HasValue)
            {
                query = query.Where(ar => ar.WorkDate >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(ar => ar.WorkDate <= endDate.Value.Date);
            }

            var records = await query
                .OrderByDescending(ar => ar.CheckOutTime == null) 
                .ThenByDescending(ar => ar.CheckInTime) // 再按簽到時間降序
                .Select(ar => new
                {
                    id = ar.AttendanceId,
                    status = ar.CheckOutTime != null,
                    workDate = ar.WorkDate.ToString("yyyy/MM/dd"), // 新增打卡日期
                    employeeName = ar.Employee != null && ar.Employee.EmployeeDetail != null
                        ? ar.Employee.EmployeeDetail.EmployeeName
                        : "未知",
                    employeeId = ar.EmployeeId,
                    startTime = ar.CheckInTime.ToString("HH:mm"),
                    endTime = ar.CheckOutTime.HasValue ? ar.CheckOutTime.Value.ToString("HH:mm") : "未簽退",
                    countTime = ar.CheckOutTime.HasValue ? (ar.CheckOutTime.Value - ar.CheckInTime).TotalHours.ToString("F1") : "0"
                })
                .ToListAsync();

            return Json(records);
        }

        [Route("GetLeaveRecords")]
        [HttpGet]
        public async Task<IActionResult> GetLeaveRecords(string employeeName, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.LeaveRecords
                .Include(lr => lr.Employee)
                .ThenInclude(e => e.EmployeeDetail)
                .AsQueryable();

            if (!string.IsNullOrEmpty(employeeName))
            {
                // 添加 null 檢查
                query = query.Where(lr => lr.Employee != null && lr.Employee.EmployeeDetail != null
                    && lr.Employee.EmployeeDetail.EmployeeName.Contains(employeeName));
            }
            if (startDate.HasValue)
            {
                query = query.Where(lr => lr.StartTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(lr => lr.EndTime <= endDate.Value);
            }

            var records = await query
                .OrderBy(lr => lr.LeaveStatus) // false (0) 排在 true (1) 之前
                .ThenByDescending(lr => lr.StartTime) // 再按開始時間降序
                .Select(lr => new
                {
                    id = lr.EmployeeId + lr.StartTime.ToString("yyyyMMddHHmmss"),
                    photo = lr.Employee.EmployeeDetail.EmployeePhoto != null
                        ? Convert.ToBase64String(lr.Employee.EmployeeDetail.EmployeePhoto)
                        : null,
                    employeeName = lr.Employee.EmployeeDetail.EmployeeName,
                    employeeId = lr.EmployeeId,
                    startDate = lr.StartTime.ToString("yyyy/MM/dd"),
                    startTime = lr.StartTime.ToString("HH:mm"),
                    endDate = lr.EndTime.ToString("yyyy/MM/dd"),
                    endTime = lr.EndTime.ToString("HH:mm"),
                    status = lr.LeaveStatus,
                    leaveType = "特休",
                    countTime = lr.SpentHours
                })
                .ToListAsync();

            return Json(records);
        }
        [Route("UpdateLeaveStatus")]
        [HttpPost]
        public async Task<IActionResult> UpdateLeaveStatus(string employeeId, DateTime startTime, bool approve)
        {
            try
            {
                Console.WriteLine($"收到更新請求: employeeId={employeeId}, startTime={startTime}, approve={approve}");

                var leaveRecord = await _context.LeaveRecords
                    .FirstOrDefaultAsync(lr => lr.EmployeeId == employeeId && lr.StartTime == startTime);

                if (leaveRecord == null)
                {
                    Console.WriteLine("找不到記錄");
                    return Json(new { success = false, message = "找不到該請假記錄" });
                }

                leaveRecord.LeaveStatus = approve;
                await _context.SaveChangesAsync();

                Console.WriteLine("更新成功");
                return Json(new { success = true, message = approve ? "已核准請假" : "已拒絕請假" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新失敗: {ex.Message}");
                return Json(new { success = false, message = $"伺服器錯誤: {ex.Message}" });
            }
        }
        public class AttendanceViewModel
        {
            public IEnumerable<AttendanceRecord>? AttendanceRecords { get; set; }
            public IEnumerable<LeaveRecord>? LeaveRecords { get; set; }
        }
    }
}