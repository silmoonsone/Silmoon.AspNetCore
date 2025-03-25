using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Silmoon.AspNetCore.Interfaces;
using Silmoon.Extension;
using Silmoon.Extension.Models;
using Silmoon.Extension.Enums;
using Silmoon.Extension.Interfaces;

namespace Silmoon.AspNetCore.Filters
{
    public class RequireUserTokenAttribute : ActionFilterAttribute
    {
        public IdentityRole Role { get; set; }
        public string UserTokenKey { get; set; }
        public string UserTokenSignInKey { get; set; }
        public bool IsRequire { get; set; } = true;
        public bool AllowSession { get; set; } = false;

        public RequireUserTokenAttribute(IdentityRole Role, string UserTokenKey = "UserToken", string UserTokenSignInKey = "UserTokenSignIn", bool IsRequire = true, bool AllowSession = false) : base()
        {
            this.Role = Role;
            this.UserTokenKey = UserTokenKey;
            this.UserTokenSignInKey = UserTokenSignInKey;
            this.IsRequire = IsRequire;
            this.AllowSession = AllowSession;
        }
        public RequireUserTokenAttribute(string UserTokenKey = "UserToken", string UserTokenSignInKey = "UserTokenSignIn", bool IsRequire = true, bool AllowSession = false) : base()
        {
            Role = IdentityRole.Undefined;
            this.UserTokenKey = UserTokenKey;
            this.UserTokenSignInKey = UserTokenSignInKey;
            this.IsRequire = IsRequire;
            this.AllowSession = AllowSession;
        }

        public async override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var UserToken = filterContext.HttpContext.Request.Query[UserTokenKey].ToStringOrNull();
            var UserTokenSignIn = filterContext.HttpContext.Request.Query[UserTokenSignInKey].ToStringOrNull().ToBool();
            if (filterContext.HttpContext.Request.Method == "POST" && UserToken.IsNullOrEmpty()) UserToken = filterContext.HttpContext.Request.Form[UserTokenKey].ToStringOrNull();
            if (filterContext.HttpContext.Request.Method == "POST" && !UserTokenSignIn) UserTokenSignIn = filterContext.HttpContext.Request.Form[UserTokenSignInKey].ToStringOrNull().ToBool();

            var silmoonAuthService = filterContext.HttpContext.RequestServices.GetService<ISilmoonAuthService>();

            if (UserToken.IsNullOrEmpty())
            {
                if (!AllowSession || !await silmoonAuthService.IsSignIn())
                    if (IsRequire) filterContext.Result = new ContentResult() { Content = ApiResult.Create(ResultState.Fail, "Require signin(Require UserToken)", -9999).ToJsonString(), ContentType = "application/json" };
            }
            else
            {
                IDefaultUserIdentity user = await silmoonAuthService.GetUser<IDefaultUserIdentity>(UserToken);
                if (user is not null)
                {
                    if (UserTokenSignIn) await silmoonAuthService.SignIn(user);
                }
                else
                {
                    if (!AllowSession || !await silmoonAuthService.IsSignIn())
                        if (IsRequire) filterContext.Result = new ContentResult() { Content = ApiResult.Create(ResultState.Fail, "Require signin(Error UserToken)", -9999).ToJsonString(), ContentType = "application/json" };
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
