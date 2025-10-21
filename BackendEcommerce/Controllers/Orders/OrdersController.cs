using Microsoft.AspNetCore.Mvc;

namespace BackendEcommerce.Controllers.Orders
{
    public class OrdersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
