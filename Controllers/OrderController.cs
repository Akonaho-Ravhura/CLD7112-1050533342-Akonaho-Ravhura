using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ABC_Inc_Project_CLD7112.Models;
using ABC_Inc_Project_CLD7112.Models.ViewModels;
using ABC_Inc_Project_CLD7112.Services;

namespace ABC_Inc_Project_CLD7112.Controllers
{
    public class OrderController : Controller
    {
        private const string OrderPartitionKey = "Order";
        private const string CustomerPartitionKey = "Customer";
        private const string OrderQueueName = "order-processing";

        private readonly ITableStorageService _tableStorageService;
        private readonly IQueueStorageService _queueStorageService;

        public OrderController(ITableStorageService tableStorageService, IQueueStorageService queueStorageService)
        {
            _tableStorageService = tableStorageService;
            _queueStorageService = queueStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _tableStorageService.GetAllEntitiesAsync<Order>();
            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await BuildCreateViewModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel formModel)
        {
            var products = await _tableStorageService.GetAllEntitiesAsync<Product>();

            var selectedLines = new List<(Product Product, int Quantity)>();
            foreach (var line in formModel.Lines)
            {
                if (line.Quantity <= 0)
                {
                    continue;
                }

                var product = products.FirstOrDefault(p => p.RowKey == line.ProductRowKey);
                if (product == null)
                {
                    continue;
                }

                selectedLines.Add((product, line.Quantity));
            }

            if (string.IsNullOrWhiteSpace(formModel.CustomerRowKey) || selectedLines.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Select a customer and at least one product with a quantity greater than zero.");
                var rebuilt = await BuildCreateViewModelAsync();
                rebuilt.CustomerRowKey = formModel.CustomerRowKey;
                return View(rebuilt);
            }

            var customer = await _tableStorageService.GetEntityAsync<CustomerProfile>(CustomerPartitionKey, formModel.CustomerRowKey);
            if (customer == null)
            {
                return NotFound();
            }

            var orderId = Guid.NewGuid().ToString();
            var order = new Order
            {
                PartitionKey = OrderPartitionKey,
                RowKey = orderId,
                CustomerRowKey = customer.RowKey,
                CustomerName = $"{customer.FirstName} {customer.LastName}",
                OrderDate = DateTimeOffset.UtcNow,
                Status = "Pending",
                TotalAmount = selectedLines.Sum(l => l.Product.StockPrice * l.Quantity)
            };

            await _tableStorageService.AddEntityAsync(order);

            foreach (var (product, quantity) in selectedLines)
            {
                var item = new OrderItem
                {
                    PartitionKey = orderId,
                    RowKey = Guid.NewGuid().ToString(),
                    ProductRowKey = product.RowKey,
                    ProductName = product.ProductName,
                    Quantity = quantity,
                    UnitPrice = product.StockPrice,
                    LineTotal = product.StockPrice * quantity
                };

                await _tableStorageService.AddEntityAsync(item);
            }

            await SendOrderMessageAsync(order);

            return RedirectToAction(nameof(Details), new { rowKey = orderId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(string rowKey)
        {
            var order = await _tableStorageService.GetEntityAsync<Order>(OrderPartitionKey, rowKey);
            if (order == null)
            {
                return NotFound();
            }

            var items = await _tableStorageService.GetEntitiesByPartitionAsync<OrderItem>(rowKey);
            return View(new OrderDetailsViewModel { Order = order, Items = items });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string rowKey)
        {
            var order = await _tableStorageService.GetEntityAsync<Order>(OrderPartitionKey, rowKey);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order formData)
        {
            var existing = await _tableStorageService.GetEntityAsync<Order>(OrderPartitionKey, formData.RowKey);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Status = formData.Status;
            await _tableStorageService.UpdateEntityAsync(existing);

            await SendOrderMessageAsync(existing);

            return RedirectToAction(nameof(Details), new { rowKey = existing.RowKey });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string rowKey)
        {
            var order = await _tableStorageService.GetEntityAsync<Order>(OrderPartitionKey, rowKey);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Order formData)
        {
            var items = await _tableStorageService.GetEntitiesByPartitionAsync<OrderItem>(formData.RowKey);
            foreach (var item in items)
            {
                await _tableStorageService.DeleteEntityAsync<OrderItem>(item.PartitionKey, item.RowKey);
            }

            await _tableStorageService.DeleteEntityAsync<Order>(OrderPartitionKey, formData.RowKey);
            return RedirectToAction(nameof(Index));
        }

        private async Task SendOrderMessageAsync(Order order)
        {
            var message = new OrderMessage
            {
                OrderId = order.RowKey,
                Status = order.Status,
                Timestamp = DateTimeOffset.UtcNow
            };

            await _queueStorageService.SendMessageAsync(OrderQueueName, JsonSerializer.Serialize(message));
        }

        private async Task<OrderCreateViewModel> BuildCreateViewModelAsync()
        {
            var customers = await _tableStorageService.GetAllEntitiesAsync<CustomerProfile>();
            var products = await _tableStorageService.GetAllEntitiesAsync<Product>();

            return new OrderCreateViewModel
            {
                Customers = customers
                    .Select(c => new CustomerOption { RowKey = c.RowKey, DisplayName = $"{c.FirstName} {c.LastName}" })
                    .ToList(),
                Lines = products
                    .Select(p => new OrderLineInput
                    {
                        ProductRowKey = p.RowKey,
                        ProductName = p.ProductName,
                        UnitPrice = p.StockPrice,
                        AvailableStock = p.StockQuantity,
                        Quantity = 0
                    })
                    .ToList()
            };
        }
    }
}
