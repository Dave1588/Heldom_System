using Heldom_SYS.Models;

namespace Heldom_SYS.Interface
{
    public interface IPrintCategoryService
    {
        Task<List<PrintCategory>> GetAllPrintCategoriesAsync();
    }
}
