using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Silmoon.AspNetCore.Extensions;
using Silmoon.AspNetCore.Interfaces;
using Silmoon.Extension;
using Silmoon.Extension.Interfaces;
using Silmoon.Extension.Enums;
using Silmoon.Models;
using System.Web;

namespace Silmoon.AspNetCore.Filters
{
    public class RequireSessionAttribute<TUser> : ActionFilterAttribute where TUser : class, IDefaultUserIdentity
    {
        public IdentityRole Role { get; set; }
        public bool RequestRefreshUserSession { get; set; }
        public string SignInUrl { get; set; } = "~/User/Signin?url=$SigninUrl";
        public bool IsApiRequest { get; set; } = false;
        public string UserTokenKey { get; set; }
        public string UserTokenSignInKey { get; set; }
        public bool IsRequire { get; set; } = true;
        public RequireSessionAttribute(IdentityRole Role, bool IsRequire = true, bool RequestRefreshUserSession = false, bool IsApiRequest = false, string UserTokenKey = "UserToken", string UserTokenSignInKey = "UserTokenSignin", string SignInUrl = "~/User/Signin?url=$SigninUrl") : base()
        {
            this.Role = Role;
            this.RequestRefreshUserSession = RequestRefreshUserSession;
            this.UserTokenKey = UserTokenKey;
            this.IsApiRequest = IsApiRequest;
            this.UserTokenSignInKey = UserTokenSignInKey;
            this.SignInUrl = SignInUrl;
            this.IsRequire = IsRequire;
        }
        public RequireSessionAttribute(bool IsRequire = true, bool RequestRefreshUserSession = false, bool IsApiRequest = false, string UserTokenKey = "UserToken", string UserTokenSignInKey = "UserTokenSignin", string SignInUrl = "~/User/Signin?url=$SigninUrl") : base()
        {
            Role = IdentityRole.Undefined;
            this.RequestRefreshUserSession = RequestRefreshUserSession;
            this.UserTokenKey = UserTokenKey;
            this.IsApiRequest = IsApiRequest;
            this.UserTokenSignInKey = UserTokenSignInKey;
            this.SignInUrl = SignInUrl;
            this.IsRequire = IsRequire;
        }

        public async override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var UserToken = filterContext.HttpContext.Request.Query[UserTokenKey].ToStringOrNull();
            var UserTokenSignIn = filterContext.HttpContext.Request.Query[UserTokenSignInKey].ToStringOrNull().ToBool();
            var isAjax = filterContext.HttpContext.Request.IsAjaxRequest();
            var signUrl = SignInUrl?.Replace("$SigninUrl", HttpUtility.UrlEncode(filterContext.HttpContext.Request.GetRawUrl()));

            var silmoonAuthService = filterContext.HttpContext.RequestServices.GetService<ISilmoonAuthService>();

            if (await silmoonAuthService.IsSignIn())
            {
                TUser user = await silmoonAuthService.GetUser<TUser>();
                if (user is null)
                {
                    await silmoonAuthService.SignOut();
                    if (IsRequire)
                    {
                        if (isAjax || IsApiRequest)
                            filterContext.Result = new ContentResult() { Content = StateFlag.Create(success: false, -9999, "No signin(SignIn, But user data is null).").ToJsonString(), ContentType = "application/json" };
                        else
                            filterContext.Result = new RedirectResult(signUrl);
                    }
                }
                else
                {
                    if (user.Role < Role)
                    {
                        if (isAjax || IsApiRequest)
                            filterContext.Result = new ContentResult() { Content = StateFlag.Create(success: false, -9999, "Permission error.").ToJsonString(), ContentType = "application/json" };
                        else
                            filterContext.Result = new ContentResult() { Content = "Permission error." };
                    }
                }
            }
            else
            {
                if (UserTokenSignIn && !UserToken.IsNullOrEmpty())
                {
                    TUser user = await silmoonAuthService.GetUser<TUser>(UserToken);
                    if (user == null)
                    {
                        if (IsRequire)
                        {
                            if (isAjax || IsApiRequest)
                                filterContext.Result = new ContentResult() { Content = StateFlag.Create(success: false, -9999, "No signin(Error UserToken).").ToJsonString(), ContentType = "application/json" };
                            else
                                filterContext.Result = new RedirectResult(signUrl);
                        }
                    }
                    else
                    {
                        if (user.Role >= Role) await silmoonAuthService.SignIn(user);
                        else filterContext.Result = new ContentResult() { Content = "Permission error." };
                    }
                }
                else
                {
                    if (isAjax || IsApiRequest)
                        filterContext.Result = new ContentResult() { Content = StateFlag.Create(success: false, -9999, "No signin.").ToJsonString(), ContentType = "application/json" };
                    else
                        filterContext.Result = new RedirectResult(signUrl);
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
