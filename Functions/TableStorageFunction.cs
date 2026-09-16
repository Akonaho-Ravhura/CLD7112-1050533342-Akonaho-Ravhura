using System.Text.Json;
using ABC_Inc_Project_CLD7112.Models;
using ABC_Inc_Project_CLD7112.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABC_Inc_Project_CLD7112.Functions
{
    // GET  /api/customers      -> list every customer in Table Storage
    // POST /api/customers      -> store a new customer record in Table Storage
    //                             body: { "firstName": "...", "lastName": "...", "email": "...",
    //                                     "phoneNumber": "...", "shippingAddress": "..." }
    public class TableStorageFunction
    {
        private const string PartitionKey = "Customer";

        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<TableStorageFunction> _logger;

        public TableStorageFunction(ITableStorageService tableStorageService, ILogger<TableStorageFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        [Function("TableStorageFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "customers")] HttpRequest req)
        {
            if (HttpMethods.IsPost(req.Method))
            {
                using var reader = new StreamReader(req.Body);
                var body = await reader.ReadToEndAsync();

                var customer = string.IsNullOrWhiteSpace(body)
                    ? null
                    : JsonSerializer.Deserialize<CustomerProfile>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (customer == null)
                {
                    return new BadRequestObjectResult(
                        "Request body must be a JSON customer record, e.g. { \"firstName\": \"Jane\", \"lastName\": \"Doe\", \"email\": \"jane@example.com\", \"phoneNumber\": \"0821234567\", \"shippingAddress\": \"1 Main St\" }.");
                }

                customer.PartitionKey = PartitionKey;
                customer.RowKey = Guid.NewGuid().ToString();

                await _tableStorageService.AddEntityAsync(customer);
                _logger.LogInformation("Stored customer {RowKey} in Table Storage.", customer.RowKey);

                return new CreatedResult($"/api/customers/{customer.RowKey}", customer);
            }

            var customers = await _tableStorageService.GetAllEntitiesAsync<CustomerProfile>();
            return new OkObjectResult(customers);
        }
    }
}
