using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Silmoon.AspNetCore.Extensions;
using Silmoon.AspNetCore.Services.Interfaces;
using Silmoon.Extension;
using Silmoon.Extension.Interfaces;
using Silmoon.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Services
{
    public abstract class SilmoonAuthService : ISilmoonAuthService
    {
        private IHttpContextAccessor HttpContextAccessor { get; set; }
        private IServiceProvider ServiceProvider { get; set; }
        public SilmoonAuthServiceOption Options { get; private set; }
        public SilmoonAuthService(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, IOptions<SilmoonAuthServiceOption> options)
        {
            Options = options.Value;
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
                HttpContextAccessor.HttpContext.Session.Keys.Each(key =>
                {
                    if (key.StartsWith("SessionCache"))
                    {
                        HttpContextAccessor.HttpContext.Session.Remove(key);
                    }
                });
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
                    TUser user = (TUser)await GetUserData(Name, NameIdentifier);
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
            TUser result = (TUser)await GetUserData(Name, NameIdentifier, UserToken);
            return await Task.FromResult(result);
        }
        public async Task ReloadUser<TUser>() where TUser : class, IDefaultUserIdentity
        {
            var NameIdentifier = HttpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
            var Name = HttpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == nameof(IDefaultUserIdentity.Username)).FirstOrDefault()?.Value;
            TUser user = default;
            user = (TUser)await GetUserData(Name, NameIdentifier);
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

        public void SetUserSessionFlag<T>(string Name, T Data)
        {
            HttpContextAccessor.HttpContext.Session.SetString("SessionCache:Flag:" + Name, Data.ToJsonString());
        }
        public T GetUserSessionFlag<T>(string Name)
        {
            string json = HttpContextAccessor.HttpContext.Session.GetString("SessionCache:Flag:" + Name);
            if (json.IsNullOrEmpty()) return default;
            else return JsonConvert.DeserializeObject<T>(json);
        }

        void SetUserCache<TUser>(TUser User, string NameIdentifier) where TUser : class, IDefaultUserIdentity
        {
            User.Password = "##hidden###";
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

        /// <summary>
        /// 在缓存中没有找到用户信息时，调用此方法获取用户信息。
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="NameIdentifier"></param>
        /// <returns></returns>
        public abstract Task<IDefaultUserIdentity> GetUserData(string Username, string NameIdentifier);
        /// <summary>
        /// 当使用API调用接口时，需要用户信息时，使用UserToken来获取用户信息。
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="NameIdentifier"></param>
        /// <param name="UserToken"></param>
        /// <returns></returns>
        public abstract Task<IDefaultUserIdentity> GetUserData(string Username, string NameIdentifier, string UserToken);
        public abstract string PasswordHash(string password);

        public virtual async Task OnSignIn(HttpContext httpContext, RequestDelegate requestDelegate, string username, string password)
        {
            if (username.IsNullOrEmpty() || password.IsNullOrEmpty()) await httpContext.Response.WriteJObjectAsync(false.ToStateFlag("用户名或密码为空"));
            else
            {
                var gotUser = await GetUserData(username, null);
                if (gotUser is null)
                    await httpContext.Response.WriteJObjectAsync(false.ToStateFlag("用户名不存在或者密码错误"));
                else
                {
                    if (gotUser.Username == username && gotUser.Password == PasswordHash(password))
                    {
                        await SignIn(gotUser);
                        await httpContext.Response.WriteJObjectAsync(true.ToStateFlag());
                    }
                    else await httpContext.Response.WriteJObjectAsync(false.ToStateFlag("用户名不存在或者密码错误"));
                }
            }
        }
        public virtual async Task OnSignOut(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            var result = await SignOut();
            await httpContext.Response.WriteJObjectAsync(result.ToStateFlag());
        }
    }
}
