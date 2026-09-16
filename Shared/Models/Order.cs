using Azure;
using Azure.Data.Tables;

namespace ABC_Inc_Project_CLD7112.Models
{
    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string CustomerRowKey { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
        public string Status { get; set; } = "Pending";
        public double TotalAmount { get; set; }
    }
}
