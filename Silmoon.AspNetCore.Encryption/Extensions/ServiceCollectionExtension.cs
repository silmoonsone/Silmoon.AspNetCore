using Microsoft.Extensions.DependencyInjection;
using Silmoon.AspNetCore.Encryption.JsComponents;
using Silmoon.AspNetCore.Encryption.Services;
using Silmoon.AspNetCore.Encryption.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Encryption.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddWebAuthnJsInterop(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<WebAuthnComponentInterop>();
        }

        public static void AddWebAuthn<TWebAuthnService>(this IServiceCollection services) where TWebAuthnService : class, IWebAuthnService
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddSingleton<IWebAuthnService, TWebAuthnService>();
        }

        public static void AddWebAuthn<TWebAuthnService>(this IServiceCollection services, Action<WebAuthnServiceOptions> options) where TWebAuthnService : class, IWebAuthnService
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(options);

            services.Configure(options);
            services.AddSingleton<IWebAuthnService, TWebAuthnService>();
        }
        public static void AddBlazorWebAuthnService<TWebAuthnService>(this IServiceCollection services) where TWebAuthnService : BlazorWebAuthnService
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<BlazorWebAuthnService, TWebAuthnService>();
            services.AddWebAuthnJsInterop();
        }

    }
}
