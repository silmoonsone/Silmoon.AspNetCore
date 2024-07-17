using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Silmoon.AspNetCore.Extensions;
using Silmoon.Extension;
using Silmoon.Threading;
using System.Threading.Tasks;
using System;
using Silmoon.Extension.Models.Types;
using Silmoon.Extension.Models;

namespace Silmoon.AspNetCore.Filters
{
    /// <summary>
    /// RequestSignatureValidationAttribute的扩展，允许不签名，但是限制请求次数。
    /// </summary>
    public class RequestSignatureValidationTimeLimitAttribute : RequestSignatureValidationAttribute
    {
        TimeLimitList timeLimitList { get; set; } = new TimeLimitList();
        int PeriodSeconds { get; set; } = 60;
        int LimitTimes { get; set; } = 30;
        public RequestSignatureValidationTimeLimitAttribute(string KeyFieldName = "Key", string AppIdFieldName = "AppId", string SignFieldName = "Signature", bool Require = true, int PeriodSeconds = 60, int LimitTimes = 30) : base(KeyFieldName, AppIdFieldName, SignFieldName, Require)
        {
            this.PeriodSeconds = PeriodSeconds;
            this.LimitTimes = LimitTimes;
        }
        public override async Task OnActionExecutionAsync(ActionExecutingContext filterContext, ActionExecutionDelegate next)
        {
            string signature;
            if (filterContext.HttpContext.Request.Method == "POST")
                signature = filterContext.HttpContext.Request.Form[SignFieldName];
            else
                signature = filterContext.HttpContext.Request.Query[SignFieldName];
            if (signature.IsNullOrEmpty())
            {
                string ocKey = filterContext.HttpContext.GetClientIPAddress().ToString() + "_apiLimit";

                if (timeLimitList.CanDo(ocKey, TimeSpan.FromSeconds(PeriodSeconds), LimitTimes))
                    await base.OnActionExecutionAsync(filterContext, next);
                else
                    filterContext.Result = new ContentResult() { Content = ApiResult<string>.Create(ResultState.Fail, null, $"Too many requests.").ToJsonString(), ContentType = "application/json" };
            }
            else
                await base.OnActionExecutionAsync(filterContext, next);
        }
    }
}
