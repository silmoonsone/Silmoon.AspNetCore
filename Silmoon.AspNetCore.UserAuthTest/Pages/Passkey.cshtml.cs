using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Silmoon.AspNetCore.Services.Interfaces;
using Silmoon.AspNetCore.UserAuthTest.Models;
using Silmoon.AspNetCore.UserAuthTest.Models.SubModels;

namespace Silmoon.AspNetCore.UserAuthTest.Pages
{
    public class PasskeyListModel : PageModel
    {
        ISilmoonAuthService SilmoonAuthService { get; set; }
        Core Core { get; set; }
        public PasskeyListModel(ISilmoonAuthService silmoonAuthService, Core core)
        {
            Core = core;
            SilmoonAuthService = silmoonAuthService;
        }
        public async void OnGet()
        {
            var user = await SilmoonAuthService.GetUser<User>();
            var userWebAuthnInfos = Core.GetUserWebAuthnInfos(user._id);
            ViewData.Add("UserWebAuthnInfos", userWebAuthnInfos);
        }
    }
}
