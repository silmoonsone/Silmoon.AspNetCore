using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.DependencyInjection;
using Silmoon.AspNetCore.Encryption.Services.Interfaces;
using System;

namespace Silmoon.AspNetCore.Encryption.Services
{
    public static class ServiceCollectionExtension
    {
        public static void AddWebAuthn<TWebAuthnService>(this IServiceCollection services) where TWebAuthnService : WebAuthnService
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<IWebAuthnService, TWebAuthnService>();
        }

        public static void AddWebAuthn<TWebAuthnService>(this IServiceCollection services, Action<WebAuthnServiceOptions> options) where TWebAuthnService : WebAuthnService
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(options);

            services.Configure(options);
            services.AddSingleton<IWebAuthnService, TWebAuthnService>();
        }

    }
}
