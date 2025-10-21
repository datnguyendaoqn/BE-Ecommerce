using Microsoft.AspNetCore.Mvc;

namespace BackendEcommerce.Controllers.Shops
{
    public class ShopsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
