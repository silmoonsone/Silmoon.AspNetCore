using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Encryption.ClientModels;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.Models;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Encryption.Services.Interfaces
{
    public interface IWebAuthnService
    {
        WebAuthnServiceOptions Options { get; set; }
        Task GetCreateOptions(HttpContext httpContext, RequestDelegate requestDelegate);
        Task GetAuthenticateOptions(HttpContext httpContext, RequestDelegate requestDelegate);
        Task Create(HttpContext httpContext, RequestDelegate requestDelegate);
        Task Delete(HttpContext httpContext, RequestDelegate requestDelegate);
        Task Authenticate(HttpContext httpContext, RequestDelegate requestDelegate);

        Task<ClientWebAuthnOptions.ClientWebAuthnUser> GetClientOptionsWebAuthnUser(HttpContext httpContext);
        Task<AllowUserCredential> GetAllowCredentials(HttpContext httpContext, string userId);
        Task<StateSet<bool>> OnCreate(HttpContext httpContext, WebAuthnCreateResponse webAuthnCreateResponse);
        Task<StateSet<bool>> OnDelete(HttpContext httpContext, byte[] credentialId);

        Task<PublicKeyInfo> OnGetPublicKeyInfo(HttpContext httpContext, byte[] rawId, string userId);
    }
}
