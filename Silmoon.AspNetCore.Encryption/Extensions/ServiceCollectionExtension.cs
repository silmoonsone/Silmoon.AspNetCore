using Microsoft.Extensions.DependencyInjection;
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
    }
}
