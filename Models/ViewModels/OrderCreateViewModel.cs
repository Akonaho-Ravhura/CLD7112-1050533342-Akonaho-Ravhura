namespace ABC_Inc_Project_CLD7112.Models.ViewModels
{
    public class OrderCreateViewModel
    {
        public string CustomerRowKey { get; set; } = string.Empty;
        public List<CustomerOption> Customers { get; set; } = new();
        public List<OrderLineInput> Lines { get; set; } = new();
    }

    public class CustomerOption
    {
        public string RowKey { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public class OrderLineInput
    {
        public string ProductRowKey { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public double UnitPrice { get; set; }
        public int AvailableStock { get; set; }
        public int Quantity { get; set; }
    }
}
