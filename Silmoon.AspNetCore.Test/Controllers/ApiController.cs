using Microsoft.AspNetCore.Mvc;

namespace Silmoon.AspNetCore.Test.Controllers
{
    public class ApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
