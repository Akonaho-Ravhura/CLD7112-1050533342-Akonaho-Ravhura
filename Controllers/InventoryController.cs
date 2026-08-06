using Microsoft.AspNetCore.Mvc;

namespace ABC_Inc_Project_CLD7112.Controllers
{
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
