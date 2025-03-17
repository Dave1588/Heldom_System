using Heldom_SYS.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Heldom_SYS.Controllers
{
    public class ToolController : Controller
    {
        private readonly IUserStoreService UserRoleStore;
        public ToolController(IUserStoreService _UserRoleStore)
        {
            UserRoleStore = _UserRoleStore;
        }

        [HttpGet]
        public string GetMenu()
        {
            //預設角色為 A M E P X
            //UserRoleStore.SetRole("A");
            //UserRoleStore.SetRole("M");
            //UserRoleStore.SetRole("E");
            //UserRoleStore.SetRole("P");
            //UserRoleStore.SetRole("X");

            //測試拿全部
            //UserRoleStore.CreateALLMenu();
            //UserRoleStore.CreateMenu();

            return UserRoleStore.MenuStr;
        }
    }
}
