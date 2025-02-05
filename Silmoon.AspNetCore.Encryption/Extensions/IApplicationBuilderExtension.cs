using Microsoft.AspNetCore.Builder;
using System;
using Silmoon.AspNetCore.Encryption.Middlewares;

namespace Silmoon.AspNetCore.Encryption.Extensions
{
    public static class IApplicationBuilderExtension
    {
        public static void UseWebAuthn(this IApplicationBuilder app) => app.UseMiddleware<WebAuthnMiddleware>();
    }
}
