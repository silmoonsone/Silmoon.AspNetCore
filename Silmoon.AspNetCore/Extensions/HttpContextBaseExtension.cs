using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Silmoon.AspNetCore.Extensions
{
    public static class HttpContextBaseExtension
    {
        public static IPAddress GetClientIpAddress(this HttpContext httpContext)
        {
            IPAddress result = null;

            if (!string.IsNullOrEmpty(httpContext.Request.Headers["X-Forwarded-For"]))
                return IPAddress.Parse(httpContext.Request.Headers["X-Forwarded-For"].ToString().Split(new string[] { " ", ",", ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);

            if (!string.IsNullOrEmpty(httpContext.Request.Headers["CF-Connecting-IP"]))
                return IPAddress.Parse(httpContext.Request.Headers["CF-Connecting-IP"].ToString().Split(new string[] { " ", ",", ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);

            if (!string.IsNullOrEmpty(httpContext.GetServerVariable("HTTP_X_FORWARDED_FOR")))
                return IPAddress.Parse(httpContext.GetServerVariable("HTTP_X_FORWARDED_FOR").Split(new string[] { " ", ",", ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);

            if (!string.IsNullOrEmpty(httpContext.GetServerVariable("REMOTE_ADDR")))
                return IPAddress.Parse(httpContext.GetServerVariable("REMOTE_ADDR").Split(new string[] { " ", ",", ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);

            if (httpContext.Connection.RemoteIpAddress is not null) return httpContext.Connection.RemoteIpAddress;

            return result;
        }
    }
}
