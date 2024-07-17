using Microsoft.AspNetCore.Mvc;

namespace Silmoon.AspNetCore.Test.Controllers
{
    public class AppController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AppMenuItem()
        {
            return View();
        }
        public IActionResult AppInfoEditForm()
        {
            return View();
        }
    }
}
