using Silmoon.AspNetCore.Services;
using Silmoon.Extension.Models.Identities;

namespace Silmoon.AspNetCore.Test.Services
{
    public class SilmoonAuthServiceImpl : SilmoonAuthService
    {
        public Core Core { get; set; }
        public SilmoonAuthServiceImpl(Core core, IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor) : base(serviceProvider, httpContextAccessor)
        {
            Core = core;
        }
        public override IDefaultUserIdentity GetUserData(string Username, string NameIdentifier)
        {
            var user = Core.GetUser(Username);
            if (user is not null) user.Password = "#hidden#";
            return user;
        }
        public override IDefaultUserIdentity GetUserDataByUserToken(string Username, string NameIdentifier, string UserToken)
        {
            return null;
        }
    }
}
