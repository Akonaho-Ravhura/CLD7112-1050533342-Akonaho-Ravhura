using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Azure.Data.Tables;
using ABC_Inc_Project_CLD7112.Models;

namespace ABC_Inc_Project_CLD7112.Services
{
    public class SeedDataService : ISeedDataService
    {
        private const string CustomerPartitionKey = "Customer";
        private const string ProductPartitionKey = "Product";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITableStorageService _tableStorageService;

        public SeedDataService(IHttpClientFactory httpClientFactory, ITableStorageService tableStorageService)
        {
            _httpClientFactory = httpClientFactory;
            _tableStorageService = tableStorageService;
        }

        public async Task<int> SeedCustomersAsync(int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            await ClearTableAsync<CustomerProfile>();

            var client = _httpClientFactory.CreateClient("RandomUser");
            var response = await client.GetFromJsonAsync<RandomUserApiResponse>($"api/?results={count}");

            if (response?.Results == null)
            {
                return 0;
            }

            var written = 0;
            foreach (var result in response.Results)
            {
                var customer = new CustomerProfile
                {
                    PartitionKey = CustomerPartitionKey,
                    RowKey = Guid.NewGuid().ToString(),
                    FirstName = result.Name.First,
                    LastName = result.Name.Last,
                    Email = result.Email,
                    PhoneNumber = result.Phone,
                    ShippingAddress = $"{result.Location.Street.Number} {result.Location.Street.Name}, " +
                                       $"{result.Location.City}, {result.Location.State}, {result.Location.Country}"
                };

                await _tableStorageService.AddEntityAsync(customer);
                written++;
            }

            return written;
        }

        public async Task<int> SeedProductsAsync(int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            await ClearTableAsync<Product>();

            var client = _httpClientFactory.CreateClient("DummyJson");
            var response = await client.GetFromJsonAsync<DummyJsonProductsResponse>($"products?limit={count}");

            if (response?.Products == null)
            {
                return 0;
            }

            var written = 0;
            foreach (var item in response.Products)
            {
                var product = new Product
                {
                    PartitionKey = ProductPartitionKey,
                    RowKey = Guid.NewGuid().ToString(),
                    ProductName = item.Title,
                    // dummyjson has no separate "type" field distinct from category, so reuse it.
                    ProductType = item.Category,
                    ProductDescription = item.Description,
                    ProductCategory = item.Category,
                    StockQuantity = item.Stock,
                    StockPrice = item.Price,
                    ImageUrl = item.Thumbnail
                };

                await _tableStorageService.AddEntityAsync(product);
                written++;
            }

            return written;
        }

        private async Task ClearTableAsync<T>() where T : class, ITableEntity, new()
        {
            var existing = await _tableStorageService.GetAllEntitiesAsync<T>();
            foreach (var entity in existing)
            {
                await _tableStorageService.DeleteEntityAsync<T>(entity.PartitionKey, entity.RowKey);
            }
        }

        // --- External API response shapes (only the fields we need) ---

        private class RandomUserApiResponse
        {
            [JsonPropertyName("results")]
            public List<RandomUserResult> Results { get; set; } = new();
        }

        private class RandomUserResult
        {
            [JsonPropertyName("name")]
            public RandomUserName Name { get; set; } = new();

            [JsonPropertyName("email")]
            public string Email { get; set; } = string.Empty;

            [JsonPropertyName("phone")]
            public string Phone { get; set; } = string.Empty;

            [JsonPropertyName("location")]
            public RandomUserLocation Location { get; set; } = new();
        }

        private class RandomUserName
        {
            [JsonPropertyName("first")]
            public string First { get; set; } = string.Empty;

            [JsonPropertyName("last")]
            public string Last { get; set; } = string.Empty;
        }

        private class RandomUserLocation
        {
            [JsonPropertyName("street")]
            public RandomUserStreet Street { get; set; } = new();

            [JsonPropertyName("city")]
            public string City { get; set; } = string.Empty;

            [JsonPropertyName("state")]
            public string State { get; set; } = string.Empty;

            [JsonPropertyName("country")]
            public string Country { get; set; } = string.Empty;
        }

        private class RandomUserStreet
        {
            [JsonPropertyName("number")]
            public int Number { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
        }

        private class DummyJsonProductsResponse
        {
            [JsonPropertyName("products")]
            public List<DummyJsonProduct> Products { get; set; } = new();
        }

        private class DummyJsonProduct
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;

            [JsonPropertyName("category")]
            public string Category { get; set; } = string.Empty;

            [JsonPropertyName("price")]
            public double Price { get; set; }

            [JsonPropertyName("stock")]
            public int Stock { get; set; }

            [JsonPropertyName("thumbnail")]
            public string Thumbnail { get; set; } = string.Empty;
        }
    }
}
