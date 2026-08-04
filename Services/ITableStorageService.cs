using Azure.Data.Tables;

namespace ABC_Inc_Project_CLD7112.Services
{
    public interface ITableStorageService
    {
        Task AddEntityAsync<T>(T entity) where T : class, ITableEntity, new();
        Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new();
        Task<List<T>> GetAllEntitiesAsync<T>() where T : class, ITableEntity, new();
        Task<List<T>> GetEntitiesByPartitionAsync<T>(string partitionKey) where T : class, ITableEntity, new();
        Task UpdateEntityAsync<T>(T entity) where T : class, ITableEntity, new();
        Task DeleteEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new();
    }
}