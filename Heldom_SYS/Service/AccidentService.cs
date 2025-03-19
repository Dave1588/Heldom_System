using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Heldom_SYS.CustomModel;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using NPOI.SS.Formula.Functions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Azure;
using System.Collections.Generic;

namespace Heldom_SYS.Service
{
    public class AccidentService: IAccidentService
    {
        private readonly SqlConnection DataBase;
        private readonly IUserStoreService UserRoleStore;

        public AccidentService(SqlConnection connection, IUserStoreService _UserRoleStore)
        {
            DataBase = connection;
            UserRoleStore = _UserRoleStore;
        }

        public async Task<IEnumerable<Accident>>  GetReport(AccidentReq req)
        {
            string sql = @"SELECT * FROM Accident
                    where EmployeeID = @EmployeeID
                    ORDER BY AccidentID DESC
                    OFFSET( @Page - 1) * 10 ROWS
                    FETCH NEXT 10 ROWS ONLY";


            IEnumerable<Accident>? data = await DataBase.QueryAsync<Accident>(sql,
                new
                {
                    EmployeeID = UserRoleStore.UserID,
                    Page = req.Page
                });

            return data;
        }



        public class PageData
        {
            public required string Total { get; set; }
        }

        public async Task<int> GetReportPage()
        {
            string sql = @"SELECT count(*) as Total FROM Accident
                        where EmployeeID = @EmployeeID";

            IEnumerable<PageData>? data = await DataBase.QueryAsync<PageData>(sql,
                new
                {
                    EmployeeID = UserRoleStore.UserID,
                });

            var result = data.Select(x => x.Total).ToList().First();

            int count = (int)Math.Ceiling(int.Parse(result) / 10.0);

            return count;
        }

        public async Task<IEnumerable<AccidentRes>> GetTrack(AccidentReq req)
        {
            // 需加入日期比對和人名join判斷
            string sql = @"SELECT Accident.*,EmployeeDetail.EmployeeName FROM Accident
                    left join EmployeeDetail on Accident.EmployeeID = EmployeeDetail.EmployeeID
                    where IncidentControllerID = @IncidentControllerID
                    ";
            
            if (!req.Title.IsNullOrEmpty()) {
                sql += @" and AccidentTitle like @AccidentTitle";
            }
            
            if (!req.Type.IsNullOrEmpty() && !(req.Type == "全部"))
            {
                sql += @" and AccidentType = @AccidentType";
            }

            if (!req.Name.IsNullOrEmpty())
            {
                sql += @" and EmployeeName like @EmployeeName";
            }

            if (!req.Date.IsNullOrEmpty())
            {
                sql += @" and((StartTime < @Date AND EndTime >= @Date) OR StartTime < @Date)";
            }
            

            sql += @" ORDER BY AccidentID DESC
                    OFFSET( @Page - 1) * 10 ROWS
                    FETCH NEXT 10 ROWS ONLY";

            IEnumerable<AccidentRes>? data = await DataBase.QueryAsync<AccidentRes>(sql,
                new
                {
                    IncidentControllerID = UserRoleStore.UserID,
                    Page = req.Page,
                    AccidentTitle = $"%{req.Title}%",
                    AccidentType = req.Type,
                    EmployeeName = $"%{req.Name}%",
                    Date = req.Date,
                });

            return data;
        }

        public async Task<int> GetTrackPage(AccidentReq req)
        {
            string sql = @"SELECT count(*) as Total FROM Accident
                        left join EmployeeDetail on Accident.EmployeeID = EmployeeDetail.EmployeeID
                        where IncidentControllerID = @IncidentControllerID";

            if (!req.Title.IsNullOrEmpty())
            {
                sql += @" and AccidentTitle like @AccidentTitle";
            }

            if (!req.Type.IsNullOrEmpty() && !(req.Type == "全部"))
            {
                sql += @" and AccidentType like @AccidentType";
            }

            if (!req.Name.IsNullOrEmpty())
            {
                sql += @" and EmployeeName like @EmployeeName";
            }

            if (!req.Date.IsNullOrEmpty())
            {
                sql += @" and((StartTime < @Date AND EndTime >= @Date) OR StartTime < @Date)";
            }

            IEnumerable<PageData>? data = await DataBase.QueryAsync<PageData>(sql,
                new
                {
                    IncidentControllerID = UserRoleStore.UserID,
                    AccidentTitle = $"%{req.Title}%",
                    AccidentType = req.Type,
                    EmployeeName = $"%{req.Name}%",
                    Date = req.Date,
                });

            var result = data.Select(x => x.Total).ToList().First();

            int count = (int)Math.Ceiling(int.Parse(result) / 10.0);

            return count;
        }


        public async Task<Accident> GetDetail(string id)
        {
            string sql = @"SELECT * FROM Accident where AccidentID = @AccidentID";


            IEnumerable<Accident>? data = await DataBase.QueryAsync<Accident>(sql,
                new
                {
                    AccidentID = id
                });


            return data.First();
        }

        public class MaxData
        {
            public required string AccidentID { get; set; }
        }

        public class IncidentData
        {
            public required string ImmediateSupervisor { get; set; }
        }
        public async Task AddAccident(AccidentAdd req)
        {
            string checkIDSql = @"SELECT TOP 1 AccidentID FROM Accident ORDER BY AccidentID DESC";

            IEnumerable<MaxData>? dataID = await DataBase.QueryAsync<MaxData>(checkIDSql);
            
            string resultID = dataID.Select(x => x.AccidentID).ToList().First().ToString();
            int count = int.Parse(resultID.Substring(2)) + 1;
            string AccidentID = "AC" + count.ToString().PadLeft(3,'0');


            string checkIncidentSql = @"SELECT ImmediateSupervisor FROM EmployeeDetail where EmployeeID = @EmployeeID";
            IEnumerable<IncidentData>? dataIncident = await DataBase.QueryAsync<IncidentData>(checkIncidentSql, new {
                EmployeeID = UserRoleStore.UserID
            });
            string resultIncident = dataIncident.Select(x => x.ImmediateSupervisor).ToList().First().ToString();



            string addSql = @"INSERT INTO Accident 
            ([AccidentID], [AccidentType], [AccidentTitle], [Description], [StartTime], [EmployeeID], [UploadTime], [IncidentControllerID], [Response], [EndTime], [IncidentStatus]) VALUES
            (@AccidentID, @AccidentType, @AccidentTitle, @Description, @StartTime, @EmployeeID, @StartTime, @IncidentControllerID, null, @EndTime, 0)";

            int? dataAdd = await DataBase.QuerySingleOrDefaultAsync<int>(addSql, new
            {
                AccidentID = AccidentID,
                AccidentType = req.AccidentType,
                AccidentTitle = req.AccidentTitle,
                Description = req.Description,
                StartTime = req.StartTime,
                EmployeeID = UserRoleStore.UserID,
                UploadTime = req.StartTime,
                IncidentControllerID = resultIncident,
                EndTime = req?.EndTime ?? (object)DBNull.Value,
            });


        }
        
    }
}
