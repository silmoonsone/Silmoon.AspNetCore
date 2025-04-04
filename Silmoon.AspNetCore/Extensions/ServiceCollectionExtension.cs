﻿using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.DependencyInjection;
using Silmoon.AspNetCore.Services;
using Silmoon.AspNetCore.Interfaces;
using System;

namespace Silmoon.AspNetCore.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddSilmoonAuth<TSilmoonAuthService>(this IServiceCollection services) where TSilmoonAuthService : class, ISilmoonAuthService
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddHttpContextAccessor();
            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
            services.AddSingleton<ISilmoonAuthService, TSilmoonAuthService>();
        }
        public static void AddSilmoonAuth<TSilmoonAuthService>(this IServiceCollection services, Action<SilmoonAuthServiceOption> options) where TSilmoonAuthService : class, ISilmoonAuthService
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddHttpContextAccessor();
            services.Configure(options);
            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
            services.AddSingleton<ISilmoonAuthService, TSilmoonAuthService>();
        }

        public static void AddSilmoonConfigure<TSilmoonConfigureService>(this IServiceCollection services) where TSilmoonConfigureService : class, ISilmoonConfigureService
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<ISilmoonConfigureService, TSilmoonConfigureService>();
        }
        public static void AddSilmoonConfigure<TSilmoonConfigureService>(this IServiceCollection services, Action<SilmoonConfigureServiceOption> option) where TSilmoonConfigureService : class, ISilmoonConfigureService
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(option);

            services.Configure(option);
            services.AddSingleton<ISilmoonConfigureService, TSilmoonConfigureService>();
        }
        public static void AddSilmoonConfigure(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddSingleton<ISilmoonConfigureService, SilmoonConfigureService>();
        }
        public static void AddSilmoonConfigure(this IServiceCollection services, Action<SilmoonConfigureServiceOption> option)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(option);

            services.Configure(option);
            services.AddSingleton<ISilmoonConfigureService, SilmoonConfigureService>();
        }
    }
}
