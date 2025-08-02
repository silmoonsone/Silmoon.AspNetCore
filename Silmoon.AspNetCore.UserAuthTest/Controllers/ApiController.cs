using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Silmoon.AspNetCore.Encryption.Services.Interfaces;
using Silmoon.AspNetCore.Extensions;
using Silmoon.AspNetCore.UserAuthTest.Models;
using Silmoon.AspNetCore.UserAuthTest.Models.SubModels;
using Silmoon.Secure;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.UserAuthTest.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Test()
        {
            return Ok("Test");
        }
        [HttpPost]
        public IActionResult SignUp([FromForm] string Username, [FromForm] string Password, [FromServices] Core core)
        {
            var user = new User()
            {
                Username = Username,
                Password = Password.GetMD5Hash()
            };
            var result = core.AddUser(user);
            return this.JsonStateFlag(result.State, result.Message);
        }
        [HttpPost]
        public async Task<IActionResult> ValidateWebAuthnData([FromForm] string webAuthnData, [FromServices] IWebAuthnService webAuthnService, [FromForm] string challenge = null)
        {
            var result = await webAuthnService.ValidateData(HttpContext, webAuthnData, challenge);
            return this.JsonStateFlag(result.State, result.Message);
        }
    }
}
