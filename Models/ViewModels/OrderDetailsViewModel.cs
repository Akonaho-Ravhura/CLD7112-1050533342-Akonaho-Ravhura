namespace ABC_Inc_Project_CLD7112.Models.ViewModels
{
    public class OrderDetailsViewModel
    {
        public Order Order { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new();
    }
}
