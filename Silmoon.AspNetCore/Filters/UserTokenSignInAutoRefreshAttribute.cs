using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Silmoon.Extension;
using Silmoon.AspNetCore.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Web;

namespace Silmoon.AspNetCore.Filters
{
    /// <summary>
    /// 此行为必须在验证过滤器【<see cref="Silmoon.AspNetCore.Filters.RequireSessionAttribute{TUser}"/>】之后使用！<br />
    /// 实现使用<c>UserToken</c>登录的时候，如果是没有登录并且有<c>UserTokenSignIn</c>、<c>UserToken</c>两个参数，就会停止接下来的控制器处理，并且重新刷新一次页面。<br />
    /// 重新刷新后会删除<c>UserTokenSignIn</c>、<c>UserToken</c>两个请求参数。<br />
    /// </summary>
    public class UserTokenSignInAutoRefreshAttribute : ActionFilterAttribute
    {
        public async override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var silmoonAuthService = filterContext.HttpContext.RequestServices.GetService<ISilmoonAuthService>();
            if (!await silmoonAuthService.IsSignIn() && filterContext.HttpContext.Request.Query["UserTokenSignIn"].ToString().ToBool() && !filterContext.HttpContext.Request.Query["UserToken"].ToString().IsNullOrEmpty())
            {
                var queryString = filterContext.HttpContext.Request.QueryString.ToString();
                var query = HttpUtility.ParseQueryString(queryString);
                query.Remove("UserTokenSignIn");
                query.Remove("UserToken");

                queryString = query.ToString();

                filterContext.Result = new RedirectResult((string)filterContext.HttpContext.Request.RouteValues["action"] + queryString);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
