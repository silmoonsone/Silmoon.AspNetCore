using Microsoft.Extensions.Options;
using Silmoon.AspNetCore.Services;
using Silmoon.Extension.Interfaces;
using Silmoon.Extension.Models;
using Silmoon.Secure;

namespace Silmoon.AspNetCore.Test.Services
{
    public class SilmoonAuthServiceImpl : SilmoonAuthService
    {
        public Core Core { get; set; }
        public SilmoonAuthServiceImpl(Core core, IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, IOptions<SilmoonAuthServiceOption> options) : base(serviceProvider, httpContextAccessor, options)
        {
            Core = core;
        }
        public override async Task<IDefaultUserIdentity> GetUserData(string Username, string NameIdentifier)
        {
            var user = Core.GetUser(Username);
            return await Task.FromResult(user);
        }
        public override async Task<IDefaultUserIdentity> GetUserData(string Username, string NameIdentifier, string UserToken)
        {
            return await Task.FromResult<DefaultUserIdentity>(default);
        }
        public override string PasswordHash(string password)
        {
            return password.GetMD5Hash();
        }
    }
}
