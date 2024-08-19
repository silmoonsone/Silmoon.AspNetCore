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
        Task GetWebAuthnOptions(HttpContext httpContext, RequestDelegate requestDelegate);
        Task GetWebAuthnAssertionOptions(HttpContext httpContext, RequestDelegate requestDelegate);
        Task CreateWebAuthn(HttpContext httpContext, RequestDelegate requestDelegate);
        Task DeleteWebAuthn(HttpContext httpContext, RequestDelegate requestDelegate);
        Task VerifyWebAuthn(HttpContext httpContext, RequestDelegate requestDelegate);

        Task<ClientWebAuthnOptions.ClientWebAuthnUser> GetClientOptionsWebAuthnUser(HttpContext httpContext);
        Task<AllowUserCredential> GetAllowCredentials(HttpContext httpContext, string userId);
        Task<StateSet<bool>> OnCreateWebAuthn(HttpContext httpContext, AttestationObjectData attestationObjectData, string clientDataJSON, byte[] attestationObjectByteArray, string authenticatorAttachment);
        Task<StateSet<bool>> OnDeleteWebAuthn(HttpContext httpContext, byte[] credentialId);

        Task<PublicKeyInfo> OnGetPublicKeyInfo(HttpContext httpContext, byte[] rawId, string userId);
    }
}
