namespace ABC_Inc_Project_CLD7112.Services
{
    public interface ISeedDataService
    {
        /// <summary>
        /// Clears the Customer table and repopulates it with mock data from randomuser.me.
        /// </summary>
        /// <returns>The number of customer records written.</returns>
        Task<int> SeedCustomersAsync(int count);

        /// <summary>
        /// Clears the Product table and repopulates it with mock data from dummyjson.com.
        /// </summary>
        /// <returns>The number of product records written.</returns>
        Task<int> SeedProductsAsync(int count);
    }
}
