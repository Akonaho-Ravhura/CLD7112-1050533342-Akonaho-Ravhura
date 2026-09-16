namespace ABC_Inc_Project_CLD7112.Models
{
    public class OrderMessage
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // e.g. "Processing order"
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}