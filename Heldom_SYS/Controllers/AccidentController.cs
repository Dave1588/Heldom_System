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

            var response = new
            {
                data = await AccidentService.GetReport(data),
                pageCount = await AccidentService.GetReportPage()
            };

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> GetTrack([FromBody] AccidentReq data)
        {

            var response = new
            {
                data = await AccidentService.GetTrack(data),
                pageCount = await AccidentService.GetTrackPage(data)
            };

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }


        [HttpPost]
        public async Task<IActionResult> GetDetail([FromBody] AccidentDetailReq data)
        {
       
            var response = new
            {
                data = await AccidentService.GetDetail(data.ID),
            };

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> AddAccident([FromBody] AccidentAdd req)
        {
            await AccidentService.AddAccident(req);
            var response = new
            {
                data = "新增成功"
            };

            string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            return Content(jsonResponse, "application/json");
        }
    }
}
