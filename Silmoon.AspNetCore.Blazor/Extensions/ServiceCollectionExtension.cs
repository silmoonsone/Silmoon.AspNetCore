﻿using Microsoft.Extensions.DependencyInjection;
using Silmoon.AspNetCore.Blazor.JsComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Blazor.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddJsComponentInterop(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<JsComponentInterop>();
        }
        public static void AddJsSilmoonAuthInterop(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<JsSilmoonAuthInterop>();
        }
    }
}
