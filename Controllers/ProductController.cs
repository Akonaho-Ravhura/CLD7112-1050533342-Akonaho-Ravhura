using Microsoft.AspNetCore.Mvc;
using ABC_Inc_Project_CLD7112.Models;
using ABC_Inc_Project_CLD7112.Services;

namespace ABC_Inc_Project_CLD7112.Controllers
{
    public class ProductController : Controller
    {
        private const string PartitionKey = "Product";

        private readonly ITableStorageService _tableStorageService;
        private readonly IBlobStorageService _blobStorageService;

        public ProductController(ITableStorageService tableStorageService, IBlobStorageService blobStorageService)
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _tableStorageService.GetAllEntitiesAsync<Product>();
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            if (imageFile is { Length: > 0 })
            {
                var blobName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                using var stream = imageFile.OpenReadStream();
                product.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobName, imageFile.ContentType);
            }

            product.PartitionKey = PartitionKey;
            product.RowKey = Guid.NewGuid().ToString();

            await _tableStorageService.AddEntityAsync(product);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string rowKey)
        {
            var product = await _tableStorageService.GetEntityAsync<Product>(PartitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product formData, IFormFile? imageFile)
        {
            var existing = await _tableStorageService.GetEntityAsync<Product>(PartitionKey, formData.RowKey);
            if (existing == null)
            {
                return NotFound();
            }

            existing.ProductName = formData.ProductName;
            existing.ProductType = formData.ProductType;
            existing.ProductDescription = formData.ProductDescription;
            existing.ProductCategory = formData.ProductCategory;
            existing.StockQuantity = formData.StockQuantity;
            existing.StockPrice = formData.StockPrice;

            if (imageFile is { Length: > 0 })
            {
                if (!string.IsNullOrEmpty(existing.ImageUrl))
                {
                    await _blobStorageService.DeleteImageAsync(GetBlobNameFromUrl(existing.ImageUrl));
                }

                var blobName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                using var stream = imageFile.OpenReadStream();
                existing.ImageUrl = await _blobStorageService.UploadImageAsync(stream, blobName, imageFile.ContentType);
            }

            await _tableStorageService.UpdateEntityAsync(existing);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string rowKey)
        {
            var product = await _tableStorageService.GetEntityAsync<Product>(PartitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Product formData)
        {
            var existing = await _tableStorageService.GetEntityAsync<Product>(PartitionKey, formData.RowKey);
            if (existing != null && !string.IsNullOrEmpty(existing.ImageUrl))
            {
                await _blobStorageService.DeleteImageAsync(GetBlobNameFromUrl(existing.ImageUrl));
            }

            await _tableStorageService.DeleteEntityAsync<Product>(PartitionKey, formData.RowKey);
            return RedirectToAction(nameof(Index));
        }

        private static string GetBlobNameFromUrl(string url)
        {
            return Path.GetFileName(new Uri(url).LocalPath);
        }
    }
}
