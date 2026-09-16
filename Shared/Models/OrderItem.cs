using Azure;
using Azure.Data.Tables;

namespace ABC_Inc_Project_CLD7112.Models
{
    // PartitionKey is set to the owning Order's RowKey, so every line item
    // for an order lives in the same partition and can be fetched together.
    public class OrderItem : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string ProductRowKey { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double LineTotal { get; set; }
    }
}
