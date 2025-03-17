using Heldom_SYS.Interface;
using Heldom_SYS.Models;
using Microsoft.Data.SqlClient;

namespace Heldom_SYS.Service
{
    public class PrintCategoryService : IPrintCategoryService
    {
        private readonly SqlConnection DataBase;

        public PrintCategoryService(SqlConnection connection)
        {
            DataBase = connection;
        }
        // method
        public async Task<List<PrintCategory>> GetAllPrintCategoriesAsync()
        {
            var printCategories = new List<PrintCategory>();
            string sql = "SELECT PrintCategoryID, PrintCategory FROM PrintCategory";

            using (SqlCommand command = new SqlCommand(sql, DataBase))
            {
                await DataBase.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        printCategories.Add(new PrintCategory
                        {
                            PrintCategoryId = reader.GetString(0),
                            PrintCategory1 = reader.GetString(1)
                        });
                    }
                }
                DataBase.Close();
            }

            return printCategories;
        }
    }
}
