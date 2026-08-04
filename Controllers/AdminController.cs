using Microsoft.AspNetCore.Mvc;
using ABC_Inc_Project_CLD7112.Services;

namespace ABC_Inc_Project_CLD7112.Controllers
{
    // Development-only tooling. Every action 404s outside the Development environment
    // so this can never be reached in production.
    public class AdminController : Controller
    {
        private readonly ISeedDataService _seedDataService;
        private readonly IWebHostEnvironment _env;

        public AdminController(ISeedDataService seedDataService, IWebHostEnvironment env)
        {
            _seedDataService = seedDataService;
            _env = env;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!_env.IsDevelopment())
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Seed(int customers = 20, int products = 20)
        {
            if (!_env.IsDevelopment())
            {
                return NotFound();
            }

            var customerCount = await _seedDataService.SeedCustomersAsync(customers);
            var productCount = await _seedDataService.SeedProductsAsync(products);

            TempData["SeedResult"] = $"Seeded {customerCount} customer(s) and {productCount} product(s).";
            return RedirectToAction(nameof(Index));
        }
    }
}
