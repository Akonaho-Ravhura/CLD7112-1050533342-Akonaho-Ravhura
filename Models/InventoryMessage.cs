namespace ABC_Inc_Project_CLD7112.Models
{
    public class InventoryMessage
    {
        public string ProductId { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty; // e.g. "imageName"
        public string Action { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}