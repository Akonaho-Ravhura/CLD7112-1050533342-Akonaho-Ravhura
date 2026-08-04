using Microsoft.AspNetCore.Mvc;
using ABC_Inc_Project_CLD7112.Models;
using ABC_Inc_Project_CLD7112.Services;

namespace ABC_Inc_Project_CLD7112.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ITableStorageService _tableStorageService;
        private const string PartitionKey = "Customer";

        public CustomerController(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _tableStorageService.GetAllEntitiesAsync<CustomerProfile>();
            return View(customers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfile customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            customer.PartitionKey = PartitionKey;
            customer.RowKey = Guid.NewGuid().ToString();

            await _tableStorageService.AddEntityAsync(customer);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string rowKey)
        {
            var customer = await _tableStorageService.GetEntityAsync<CustomerProfile>(PartitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerProfile formData)
        {
            var existing = await _tableStorageService.GetEntityAsync<CustomerProfile>(PartitionKey, formData.RowKey);
            if (existing == null)
            {
                return NotFound();
            }

            existing.FirstName = formData.FirstName;
            existing.LastName = formData.LastName;
            existing.Email = formData.Email;
            existing.PhoneNumber = formData.PhoneNumber;
            existing.ShippingAddress = formData.ShippingAddress;

            await _tableStorageService.UpdateEntityAsync(existing);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string rowKey)
        {
            var customer = await _tableStorageService.GetEntityAsync<CustomerProfile>(PartitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(CustomerProfile formData)
        {
            await _tableStorageService.DeleteEntityAsync<CustomerProfile>(PartitionKey, formData.RowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}