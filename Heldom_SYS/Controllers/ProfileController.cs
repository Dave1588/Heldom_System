﻿using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Heldom_SYS.CustomModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Heldom_SYS.Service;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Heldom_SYS.Controllers
{
    [Route("[controller]")]
    public class ProfileController : Controller
    {
        private readonly SqlConnection DataBase;
        private readonly IUserStoreService UserRoleStore;
        private readonly ConstructionDbContext DbContext;
        private readonly IProfileService ProfileService;
        public ProfileController(SqlConnection connection, IUserStoreService _UserRoleStore, ConstructionDbContext dbContext, IProfileService _ProfileService)
        {
            DataBase = connection;
            UserRoleStore = _UserRoleStore;
            DbContext = dbContext;
            ProfileService = _ProfileService;
        }

        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("Settings")]
        public IActionResult Settings()
        {
            return View();
        }

        [HttpGet]
        [Route("Account")]
        public IActionResult Account()
        {
            ViewBag.role = UserRoleStore.Role;
            return View();
        }

        [HttpGet]
        [Route("NewAccount")]
        public IActionResult NewAccount()
        {
            return View();
        }

        [HttpGet]
        [Route("NewAccount/{id}")]
        public IActionResult NewAccount(string id)
        {
            return View();
        }

        [HttpGet]
        [Route("GetIndexData")]
        public async Task<IActionResult> GetIndexData()
        {
            var result = await ProfileService.GetIndexData();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetSettingsData")]
        public async Task<IActionResult> GetSettingsData()
        {
            var result = await ProfileService.GetSettingsData();
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateSettingsData")]
        public async Task<IActionResult> UpdateSettingsData([FromBody] ProfileSettings userInput)
        {
            bool result = await ProfileService.UpdateSettingsData(userInput);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetAccountsData")]
        public async Task<IActionResult> GetAccountsData([FromBody] ProfileOptions options)
        {
            var response = new
            {
                data = await ProfileService.GetAccountsData(options),
                totalPage = await ProfileService.GetTotalPage(options)
            };
            return Ok(response);
        }
    }
}
