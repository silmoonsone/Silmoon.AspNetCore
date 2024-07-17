using Microsoft.AspNetCore.Mvc;
using Silmoon.AspNetCore.Services.Interfaces;

namespace Silmoon.AspNetCore.Test.Controllers
{
    public class UserController : Controller
    {
        ISilmoonAuthService SilmoonAuthService { get; set; }
        public UserController(ISilmoonAuthService silmoonAuthService)
        {
            SilmoonAuthService = silmoonAuthService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> SignIn(string Url)
        {
            if (await SilmoonAuthService.IsSignIn()) return Redirect(Url ?? "~/");
            return View();
        }
        public IActionResult SignUp()
        {
            return View();
        }
    }
}
