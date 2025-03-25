using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Dapper;
using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Heldom_SYS.CustomModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static NPOI.HSSF.Util.HSSFColor;
using System.Collections.Generic;
using System.Dynamic;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Asn1.Ocsp;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Heldom_SYS.Controllers
{
    public class AccidentController : Controller
    {
        private readonly IAccidentService AccidentService;
        public AccidentController(IAccidentService _AccidentService)
        {
            AccidentService = _AccidentService;
        }


        [HttpPost]
        public async Task<IActionResult> GetReport([FromBody] AccidentReq data)
        {
            dynamic response = new ExpandoObject();
            response.data = "未動作";
            try
            {
                response.data = await AccidentService.GetReport(data);
                response.pageCount = await AccidentService.GetReportPage();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                response.data = "拿取失敗";
            }

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }


        [HttpPost]
        public async Task<IActionResult> GetTrack([FromBody] AccidentReq data)
        {
            dynamic response = new ExpandoObject();
            response.data = "未動作";
            try
            {
                response.data = await AccidentService.GetTrack(data);
                response.pageCount = await AccidentService.GetTrackPage(data);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                response.data = "拿取失敗";
            }


            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }


        [HttpPost]
        public async Task<IActionResult> GetDetail([FromBody] AccidentDetailReq data)
        {
            dynamic response = new ExpandoObject();
            response.data = "未動作";
            try
            {
                response.data = await AccidentService.GetDetail(data.ID);
                response.reportImg = await AccidentService.GetDetailFile(data.ID,false);
                response.trackImg = await AccidentService.GetDetailFile(data.ID,true);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                response.data = "拿取失敗";
            }

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> AddAccident([FromForm] string AccidentType, [FromForm] string AccidentTitle, [FromForm] string Description, [FromForm] string StartTime, [FromForm] string EndTime, [FromForm] string Id, [FromForm] List<string> Files)
        {
            dynamic response = new ExpandoObject();
            response.data = "未動作";

            try
            {
                await AccidentService.AddAccident(AccidentType,AccidentTitle,Description,StartTime,EndTime,Id,Files);
                response.data = "新增成功";
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                response.data = "新增失敗";
            }


            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> AddReply([FromForm] string Reply, [FromForm] string AccidentId, [FromForm] string Type, [FromForm] string Id, [FromForm] List<string> Files)
        {

            dynamic response = new ExpandoObject();
            response.data = "未動作";

            try
            {
                await AccidentService.AddReply(Reply,AccidentId, Files);
                response.data = "修改成功";

            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                response.data = "修改失敗";

            }

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");

        }

    }
}
