using Azure;
using Azure.Data.Tables;

namespace ABC_Inc_Project_CLD7112.Services
{
    public class TableStorageService : ITableStorageService
    {
        private readonly TableServiceClient _serviceClient;

        public TableStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            _serviceClient = new TableServiceClient(connectionString);
        }

        private async Task<TableClient> GetTableClientAsync<T>() where T : class, ITableEntity, new()
        {
            var tableClient = _serviceClient.GetTableClient(typeof(T).Name);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        public async Task AddEntityAsync<T>(T entity) where T : class, ITableEntity, new()
        {
            var table = await GetTableClientAsync<T>();
            await table.AddEntityAsync(entity);
        }

        public async Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var table = await GetTableClientAsync<T>();
            try
            {
                var response = await table.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<List<T>> GetAllEntitiesAsync<T>() where T : class, ITableEntity, new()
        {
            var table = await GetTableClientAsync<T>();
            var results = new List<T>();

            await foreach (var entity in table.QueryAsync<T>())
            {
                results.Add(entity);
            }

            return results;
        }

        public async Task<List<T>> GetEntitiesByPartitionAsync<T>(string partitionKey) where T : class, ITableEntity, new()
        {
            var table = await GetTableClientAsync<T>();
            var results = new List<T>();

            await foreach (var entity in table.QueryAsync<T>(e => e.PartitionKey == partitionKey))
            {
                results.Add(entity);
            }

            return results;
        }

        public async Task UpdateEntityAsync<T>(T entity) where T : class, ITableEntity, new()
        {
            var table = await GetTableClientAsync<T>();
            await table.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var table = await GetTableClientAsync<T>();
            await table.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}