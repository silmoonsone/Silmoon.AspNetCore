using Microsoft.AspNetCore.Mvc;

namespace Silmoon.AspNetCore.Test.Controllers
{
    public class DemoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Editor()
        {
            return View();
        }
        public IActionResult SignalR()
        {
            return View();
        }
    }
}
