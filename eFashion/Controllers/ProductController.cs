using Microsoft.AspNetCore.Mvc;

namespace eFashion.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
