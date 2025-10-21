using Microsoft.AspNetCore.Mvc;

namespace BackendEcommerce.Controllers.Auth
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
