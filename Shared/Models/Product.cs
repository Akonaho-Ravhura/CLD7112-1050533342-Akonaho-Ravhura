using Azure;
using Azure.Data.Tables;

namespace ABC_Inc_Project_CLD7112.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public string ProductCategory { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public double StockPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}