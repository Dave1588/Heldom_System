using Heldom_SYS.Controllers;
using Heldom_SYS.CustomModel;
using Microsoft.AspNetCore.Mvc;

namespace Heldom_SYS.Interface
{
    public interface IProfileService
    {
        Task<IEnumerable<ProfileIndex>> GetIndexData();
        Task<IEnumerable<ProfileSettings>> GetSettingsData();
        Task<bool> UpdateSettingsData(ProfileSettings userInput);
        Task<IEnumerable<ProfileAccount>> GetAccountsData(ProfileOptions options);
        Task<int> GetTotalPage(ProfileOptions options);
        Task<string> GetNewId();
        Task<IEnumerable<ProfileNewAccountData>> GetSupervisor();
        Task<bool> CreateAccount(ProfileAccount userInput);
        Task<IEnumerable<ProfileAccount>> GetAccountData();
        Task<bool> UpdateAccount(ProfileAccount userInput);
    }
}
