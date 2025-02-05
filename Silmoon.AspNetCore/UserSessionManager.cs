using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Silmoon.Extension;
using Silmoon.Models;
using Silmoon.AspNetCore.Extensions;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Silmoon.Extension.Interfaces;

namespace Silmoon.AspNetCore
{
    [Obsolete]
    public static class UserSessionManager
    {
        /// <summary>
        /// 当丢失用户对象实体数据，将调用此事件获取用户实体
        /// </summary>
        [Obsolete]
        public static event UserSessionHanlder<IDefaultUserIdentity> OnRequestUserData;
        /// <summary>
        /// 当用户使用UserToken或者类似用途的字符串请求登录，将传递UserToken到事件，以便使用UserToken获取用户实体验证
        /// </summary>
        [Obsolete]
        public static event UserTokenHanlder<IDefaultUserIdentity> OnRequestUserToken;
        [Obsolete]
        public static async Task<bool> IsSignin(this HttpContext httpContext)
        {
            var result = await httpContext.AuthenticateAsync();
            return result.Succeeded;
        }
        [Obsolete]
        public static async Task Signin<TUser>(this HttpContext httpContext, TUser User, string NameIdentifier = null) where TUser : IDefaultUserIdentity, new()
        {
            if (User is null) throw new ArgumentNullException(nameof(User));
            if (User.Username.IsNullOrEmpty() && NameIdentifier.IsNullOrEmpty()) throw new ArgumentNullException(nameof(User.Username), "Username或者NameIdentifier必选最少一个参数。");
            NameIdentifier = NameIdentifier.IsNullOrEmpty() ? User.Username : NameIdentifier;

            var claimsIdentity = new ClaimsIdentity("Customer");
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, NameIdentifier));
            claimsIdentity.AddClaim(new Claim(nameof(IDefaultUserIdentity.Username), User.Username ?? ""));

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            SetUserCache(httpContext, User);

            await httpContext.SignInAsync(claimsPrincipal);
        }
        [Obsolete]
        public static async Task<bool> Signout(this HttpContext httpContext)
        {
            if (await IsSignin(httpContext))
            {
                var NameIdentifier = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
                var Name = httpContext.User.Claims.Where(c => c.Type == nameof(IDefaultUserIdentity.Username)).FirstOrDefault()?.Value;
                httpContext.Session.Remove("SessionCache:NameIdentifier+Username=" + NameIdentifier + "+" + Name);
                await httpContext.SignOutAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
        [Obsolete]
        public static async Task<TUser> GetUser<TUser>(this HttpContext httpContext) where TUser : IDefaultUserIdentity, new()
        {
            if (await IsSignin(httpContext))
            {
                var NameIdentifier = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
                var Name = httpContext.User.Claims.Where(c => c.Type == nameof(IDefaultUserIdentity.Username)).FirstOrDefault()?.Value;

                string json = httpContext.Session.GetString("SessionCache:NameIdentifier+Username=" + NameIdentifier + "+" + Name);
                TUser user = default;
                if (json.IsNullOrEmpty())
                {
                    user = (TUser)OnRequestUserData?.Invoke(Name, NameIdentifier, user);
                    if (user is null)
                    {
                        await Signout(httpContext);
                        return default;
                    }
                    SetUserCache(httpContext, user);
                }
                else
                    user = JsonSerializer.Deserialize<TUser>(json);
                return user;
            }
            else
                return default;
        }
        /// <summary>
        /// 使用UserToken获取用户实体数据以便验证的方法，此方法必须注册 UserSessionManager.OnRequestUserToken 事件。
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <param name="httpContext"></param>
        /// <param name="UserToken"></param>
        /// <param name="SessionSignin"></param>
        /// <param name="Name"></param>
        /// <param name="NameIdentifier"></param>
        /// <returns></returns>
        [Obsolete]
        public static async Task<TUser> GetUser<TUser>(this HttpContext httpContext, string UserToken, bool SessionSignin, string Name = null, string NameIdentifier = null) where TUser : IDefaultUserIdentity, new()
        {
            //if (OnRequestUserToken is null) throw new NullReferenceException("UserSessionManager.OnRequestUserToken 事件未注册");
            var result = (TUser)OnRequestUserToken?.Invoke(Name, NameIdentifier, UserToken);
            if (SessionSignin && result is not null)
            {
                await Signin(httpContext, result);
                SetUserCache(httpContext, result);
            }
            return result;
        }
        [Obsolete]
        public static void SetUserCache<TUser>(this HttpContext httpContext, TUser User) where TUser : IDefaultUserIdentity, new()
        {
            var NameIdentifier = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;

            string json = JsonSerializer.Serialize(User);
            httpContext.Session.SetString("SessionCache:NameIdentifier+Username=" + NameIdentifier + "+" + User.Username, json);
        }
        [Obsolete]
        public static async void RefreshUser<TUser>(this HttpContext httpContext) where TUser : IDefaultUserIdentity, new()
        {
            var NameIdentifier = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
            var Name = httpContext.User.Claims.Where(c => c.Type == nameof(IDefaultUserIdentity.Username)).FirstOrDefault()?.Value;
            TUser user = default;
            user = (TUser)OnRequestUserData?.Invoke(Name, NameIdentifier, user);
            if (user is null) await Signout(httpContext);
            else SetUserCache(httpContext, user);
        }

        [Obsolete]
        public delegate TUser UserSessionHanlder<TUser>(string Username, string NameIdentifier, TUser User);
        [Obsolete]
        public delegate TUser UserTokenHanlder<TUser>(string Username, string NameIdentifier, string UserToken);
    }
}
