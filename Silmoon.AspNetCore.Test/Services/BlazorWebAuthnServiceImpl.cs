using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Encryption.ClientModels;
using Silmoon.AspNetCore.Encryption.JsComponents;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.AspNetCore.Encryption.Services;
using Silmoon.AspNetCore.Interfaces;
using Silmoon.AspNetCore.Test.Models;
using Silmoon.Extension;
using Silmoon.Models;
using Silmoon.Runtime;
using System.Security.Claims;

namespace Silmoon.AspNetCore.Test.Services
{
    public class BlazorWebAuthnServiceImpl : BlazorWebAuthnService
    {
        Core Core { get; set; }
        public BlazorWebAuthnServiceImpl(Core core, IHttpContextAccessor httpContextAccessor, WebAuthnComponentInterop webAuthnComponentInterop) : base(httpContextAccessor, webAuthnComponentInterop)
        {
            Core = core;
        }

        public override Task<Credential[]> GetAllowCredentials(HttpContext httpContext, string userId)
        {
            var hasUserObjectId = ObjectId.TryParse(userId, out var userObjectId);
            if (hasUserObjectId)
            {
                var userWebAuthnInfos = Core.GetUserWebAuthnInfos(userObjectId);
                var credentials = userWebAuthnInfos.Select(x => new Credential() { Id = x.CredentialId.GetBase64String() }).ToArray();
                return Task.FromResult(credentials);
            }
            else
            {
                return Task.FromResult(Array.Empty<Credential>());
            }
        }
        public override Task<PublicKeyInfo> OnGetPublicKeyInfo(HttpContext httpContext, byte[] rawId, string userId = null)
        {
            var hasUserObjectId = ObjectId.TryParse(userId, out var userObjectId);

            UserAuthInfo userAuthInfo = null;
            if (hasUserObjectId) userAuthInfo = Core.GetUserAuthInfo(userObjectId);
            else userAuthInfo = Core.GetUserAuthInfo(rawId);

            if (userAuthInfo is not null)
            {
                var webAuthnInfo = userAuthInfo.WebAuthnInfos.FirstOrDefault(c => c.CredentialId.SequenceEqual(rawId));
                return Task.FromResult((PublicKeyInfo)webAuthnInfo);
            }
            else return Task.FromResult((PublicKeyInfo)null);
        }
    }
}
