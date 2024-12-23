using Microsoft.AspNetCore.Mvc;
using Silmoon.AspNetCore.Extensions;
using Silmoon.Extension.Models.Types;

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
        public IActionResult ApiTest()
        {
            this.JsonApiResult<object>(ResultState.Success, new object(), "message");
            return this.JsonApiResult(ResultState.Success, "message");
        }
    }
}
