﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Silmoon.AspNetCore.Extensions;
using Silmoon.AspNetCore.Services.Interfaces;
using Silmoon.Extension;
using Silmoon.Extension.Models.Identities;
using System.Net.Http;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Middlewares
{
    public class SilmoonAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private ISilmoonAuthService SilmoonAuthService { get; set; }

        public SilmoonAuthMiddleware(RequestDelegate next, ISilmoonAuthService silmoonAuthService)
        {
            _next = next;
            SilmoonAuthService = silmoonAuthService;
        }
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == SilmoonAuthService.Options.CreateSessionUrl)
            {
                var username = context.Request.GetQueryStringOrFormValue("Username");
                var password = context.Request.GetQueryStringOrFormValue("Password");

                await SilmoonAuthService.OnSignIn(context, _next, username, password);
            }
            else if (context.Request.Path == SilmoonAuthService.Options.ClearSessionUrl)
                await SilmoonAuthService.OnSignOut(context, _next);
            else
                await _next(context);
        }
    }
}