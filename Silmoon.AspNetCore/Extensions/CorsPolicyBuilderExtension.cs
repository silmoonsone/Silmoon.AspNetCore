using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Linq;
using System;

namespace Silmoon.AspNetCore.Extensions;

public static class CorsPolicyBuilderExtension
{
    public static CorsPolicyBuilder WithHosts(this CorsPolicyBuilder builder, params string[] hosts)
    {
        return builder.SetIsOriginAllowed(origin =>
        {
            if (origin is null || !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                return false;
            else
            {
                var requestHost = uri.Host;
                return hosts.Any(host => host switch
                {
                    var h when h.StartsWith("*.") && requestHost.EndsWith(h[2..], StringComparison.OrdinalIgnoreCase) => true,
                    var h when string.Equals(requestHost, h, StringComparison.OrdinalIgnoreCase) => true,
                    _ => false
                });
            }
        });
    }
    public static CorsPolicyBuilder AllowAll(this CorsPolicyBuilder builder) => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}
