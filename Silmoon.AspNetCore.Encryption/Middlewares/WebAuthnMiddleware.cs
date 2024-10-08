﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Encryption.Services.Interfaces;
using Silmoon.Runtime.Cache;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Encryption.Middlewares
{
    public class WebAuthnMiddleware
    {
        private RequestDelegate _next;
        public IWebAuthnService WebAuthnService { get; set; }

        public WebAuthnMiddleware(RequestDelegate next, IWebAuthnService webAuthnService)
        {
            _next = next;
            WebAuthnService = webAuthnService;
        }
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == WebAuthnService.Options.GetWebAuthnOptionsUrl)
            {
                await WebAuthnService.GetCreateOptions(context, _next);
            }
            else if (context.Request.Path == WebAuthnService.Options.CreateWebAuthnUrl)
            {
                await WebAuthnService.Create(context, _next);
            }
            else if (context.Request.Path == WebAuthnService.Options.DeleteWebAuthnUrl)
            {
                await WebAuthnService.Delete(context, _next);
            }
            else if (context.Request.Path == WebAuthnService.Options.GetWebAuthnAuthenticateOptions)
            {
                await WebAuthnService.GetAuthenticateOptions(context, _next);
            }
            else if (context.Request.Path == WebAuthnService.Options.AuthenticateWebAuthn)
            {
                await WebAuthnService.Authenticate(context, _next);
            }
            else
                await _next(context);
        }
    }
}
