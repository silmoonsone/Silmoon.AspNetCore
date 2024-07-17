using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Silmoon.AspNetCore.Services.Interfaces;
using Silmoon.Extension;
using Silmoon.Extension.Models.Identities;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Services
{
    public abstract class SilmoonAuthService : ISilmoonAuthService
    {
        public IHttpContextAccessor HttpContextAccessor { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public SilmoonAuthService(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            ServiceProvider = serviceProvider;
            HttpContextAccessor = httpContextAccessor;
        }
        public async Task SignIn<TUser>(TUser User, bool AddEnumRole) where TUser : class, IDefaultUserIdentity => await SignIn(User, AddEnumRole, (string[])null, null);
        public async Task SignIn<TUser>(TUser User, string NameIdentifier = null) where TUser : class, IDefaultUserIdentity => await SignIn(User, true, (string[])null, NameIdentifier);
        public async Task SignIn<TUser, TTCustomerRoles>(TUser User, bool AddEnumRole, TTCustomerRoles CustomerRoles, string NameIdentifier = null) where TUser : class, IDefaultUserIdentity where TTCustomerRoles : Enum => await SignIn(User, AddEnumRole, CustomerRoles.GetFlagStringArray(), NameIdentifier);
        public async Task SignIn<TUser, TTCustomerRoles>(TUser User, bool AddEnumRole, TTCustomerRoles[] CustomerRoles, string NameIdentifier = null) where TUser : class, IDefaultUserIdentity where TTCustomerRoles : Enum => await SignIn(User, AddEnumRole, CustomerRoles.GetStringArray(), NameIdentifier);
        public async Task SignIn<TUser>(TUser User, bool AddEnumRole, string[] CustomerRoles, string NameIdentifier = null) where TUser : class, IDefaultUserIdentity
        {
            if (User is null) throw new ArgumentNullException(nameof(User));
            if (User.Username.IsNullOrEmpty() && NameIdentifier.IsNullOrEmpty()) throw new ArgumentNullException(nameof(User.Username), "Username或者NameIdentifier必选最少一个参数。");

            NameIdentifier = NameIdentifier.IsNullOrEmpty() ? User.Username : NameIdentifier;

            var claimsIdentity = new ClaimsIdentity("Customer");
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, NameIdentifier));
            claimsIdentity.AddClaim(new Claim(nameof(IDefaultUserIdentity.Username), User.Username ?? ""));

            if (AddEnumRole) claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, User.Role.ToString()));

            if (!CustomerRoles.IsNullOrEmpty())
            {
                foreach (var item in CustomerRoles)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, item));
                }
            }

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContextAccessor.HttpContext.SignInAsync(claimsPrincipal);

            SetUserCache(User, NameIdentifier);
        }
        public async Task<bool> SignOut()
        {
            if (await IsSignIn())
            {
                var NameIdentifier = HttpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
                var Name = HttpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == nameof(IDefaultUserIdentity.Username)).FirstOrDefault()?.Value;
                HttpContextAccessor.HttpContext.Session.Remove("SessionCache:NameIdentifier+Username=" + NameIdentifier + "+" + Name);
                await HttpContextAccessor.HttpContext.SignOutAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<TUser> GetUser<TUser>() where TUser : class, IDefaultUserIdentity
        {
            if (await IsSignIn())
            {
                var NameIdentifier = HttpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
                var Name = HttpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == nameof(IDefaultUserIdentity.Username)).FirstOrDefault()?.Value;

                string json = HttpContextAccessor.HttpContext.Session.GetString("SessionCache:NameIdentifier+Username=" + NameIdentifier + "+" + Name);
                if (json.IsNullOrEmpty())
                {
                    TUser user = (TUser)GetUserData(Name, NameIdentifier);
                    if (user is null)
                    {
                        await SignOut();
                        return default;
                    }
                    else
                    {
                        SetUserCache(user, NameIdentifier);
                        return user;
                    }
                }
                else
                    return JsonConvert.DeserializeObject<TUser>(json);
            }
            else
                return null;
        }
        public async Task<TUser> GetUser<TUser>(string UserToken, string Name = null, string NameIdentifier = null) where TUser : class, IDefaultUserIdentity
        {
            TUser result = (TUser)GetUserDataByUserToken(Name, NameIdentifier, UserToken);
            return await Task.FromResult(result);
        }
        public async Task ReloadUser<TUser>() where TUser : class, IDefaultUserIdentity
        {
            var NameIdentifier = HttpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
            var Name = HttpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == nameof(IDefaultUserIdentity.Username)).FirstOrDefault()?.Value;
            TUser user = default;
            user = (TUser)GetUserData(Name, NameIdentifier);
            if (user is null) await SignOut();
            else SetUserCache(user, NameIdentifier);
        }
        public async Task<bool> IsSignIn()
        {
            var result = await HttpContextAccessor.HttpContext.AuthenticateAsync();
            return result.Succeeded;
        }
        public bool IsInRole(params string[] Role)
        {
            var claimsPrincipal = GetCurrentClaimsPrincipalAsync().Result;
            foreach (var item in Role)
            {
                if (claimsPrincipal.IsInRole(item)) return true;
            }
            return false;
        }
        public bool IsInRole(params Enum[] Roles) => IsInRole(Roles.GetStringArray());
        public async Task<bool> IsInRoleAsync(params string[] Role)
        {
            var claimsPrincipal = await GetCurrentClaimsPrincipalAsync();
            foreach (var item in Role)
            {
                if (claimsPrincipal.IsInRole(item)) return true;
            }
            return false;
        }
        public async Task<bool> IsInRoleAsync(params Enum[] Roles) => await IsInRoleAsync(Roles.GetStringArray());
        public async Task<string[]> GetRoles()
        {
            var claimsPrincipal = await GetCurrentClaimsPrincipalAsync();
            return claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
        }

        void SetUserCache<TUser>(TUser User, string NameIdentifier) where TUser : class, IDefaultUserIdentity
        {
            HttpContextAccessor.HttpContext.Session.SetString("SessionCache:NameIdentifier+Username=" + NameIdentifier + "+" + User.Username, User.ToJsonString());
        }
        async Task<ClaimsPrincipal> GetCurrentClaimsPrincipalAsync()
        {
            if (HttpContextAccessor.HttpContext is not null)
            {
                return HttpContextAccessor.HttpContext.User;
            }
            else
            {
                var authenticationStateProvider = ServiceProvider.GetService<AuthenticationStateProvider>();
                return (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
            }
        }

        public abstract IDefaultUserIdentity GetUserData(string Username, string NameIdentifier);
        public abstract IDefaultUserIdentity GetUserDataByUserToken(string Username, string NameIdentifier, string UserToken);
    }
}
