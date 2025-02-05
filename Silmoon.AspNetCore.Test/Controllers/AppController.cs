using Microsoft.AspNetCore.Mvc;
using Silmoon.AspNetCore.Extensions;
using Silmoon.Extension.Enums;

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
        public IActionResult TestGzipApi()
        {
            var obj = new { Name = "Silmoon", Age = 18 };

            //return this.JsonApiResult(ResultState.Success, obj, "message", true);
            return this.JsonApiResult(ResultState.Success, "message");
        }
    }
}
