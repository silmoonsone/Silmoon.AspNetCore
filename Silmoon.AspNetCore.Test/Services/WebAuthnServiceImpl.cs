using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Encryption.ClientModels;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.AspNetCore.Encryption.Services;
using Silmoon.AspNetCore.Services.Interfaces;
using Silmoon.AspNetCore.Test.Models;
using Silmoon.Extension;
using Silmoon.Models;
using System.Security.Claims;

namespace Silmoon.AspNetCore.Test.Services
{
    public class WebAuthnServiceImpl : WebAuthnService
    {
        Core Core { get; set; }
        IServiceProvider ServiceProvider { get; set; }
        public WebAuthnServiceImpl(IOptions<WebAuthnServiceOptions> options, Core core) : base(options)
        {
            Core = core;
        }

        public override async Task<ClientWebAuthnOptions.ClientWebAuthnUser> GetClientOptionsWebAuthnUser(HttpContext httpContext)
        {
            ISilmoonAuthService silmoonAuthService = httpContext.RequestServices.GetService<ISilmoonAuthService>();
            if (await silmoonAuthService.IsSignIn())
            {
                var user = await silmoonAuthService.GetUser<User>();

                var result = new ClientWebAuthnOptions.ClientWebAuthnUser()
                {
                    Name = user.Username,
                    DisplayName = $"{user.Username}{(user.Nickname.IsNullOrEmpty() ? string.Empty : $"({user.Nickname})")}",
                    Id = user._id.ToByteArray(),
                };
                return result;
            }
            else
            {
                return null;
            }
        }
        public override Task<StateSet<bool>> OnCreate(HttpContext httpContext, AttestationObjectData attestationObjectData, string clientDataJSON, byte[] attestationObjectByteArray, string authenticatorAttachment)
        {
            var sessionUserObjectId = ObjectId.Parse(httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var user = Core.GetUser(sessionUserObjectId);
            if (user is null) return Task.FromResult(false.ToStateSet("User not found"));
            else
            {
                var userWebAuthInfo = new Models.SubModels.UserWebAuthnInfo()
                {
                    AAGuid = attestationObjectData.AAGUID,
                    AttestationFormat = attestationObjectData.AttestationFormat,
                    CredentialId = attestationObjectData.CredentialId,
                    PublicKey = attestationObjectData.PublicKey,
                    PublicKeyAlgorithm = attestationObjectData.PublicKeyAlgorithm,
                    SignCount = attestationObjectData.SignCount,
                    UserVerified = attestationObjectData.UserVerified,
                    AttestationObject = attestationObjectByteArray,
                    AuthenticatorAttachment = authenticatorAttachment,
                };
                var result = Core.AddUserWebAuthnInfo(user._id, userWebAuthInfo);
                return Task.FromResult(result);
            }
        }
        public override Task<StateSet<bool>> OnDelete(HttpContext httpContext, byte[] credentialId)
        {
            var sessionUserObjectId = ObjectId.Parse(httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var user = Core.GetUser(sessionUserObjectId);
            if (user is null) return Task.FromResult(false.ToStateSet("User not found"));
            else
            {
                var result = Core.DeleteUserWebAuthnInfo(user._id, credentialId);
                if (result.State)
                    return Task.FromResult(true.ToStateSet());
                else
                    return Task.FromResult(false.ToStateSet(result.Message));
            }
        }



        public override Task<PublicKeyInfo> OnGetPublicKeyInfo(HttpContext httpContext, byte[] rawId, string userId)
        {
            var userObjectId = ObjectId.Parse(userId);
            var userWebAuthnInfos = Core.GetUserWebAuthnInfos(userObjectId);
            var credential = userWebAuthnInfos.FirstOrDefault(c => c.CredentialId.SequenceEqual(rawId));
            PublicKeyInfo publicKeyInfo = new PublicKeyInfo()
            {
                PublicKey = credential.PublicKey,
                PublicKeyAlgorithm = credential.PublicKeyAlgorithm,
            };
            return Task.FromResult(publicKeyInfo);
        }
        public override Task<AllowUserCredential> GetAllowCredentials(HttpContext httpContext, string userId)
        {
            var userObjectId = ObjectId.Parse(userId);
            var userWebAuthnInfos = Core.GetUserWebAuthnInfos(userObjectId);

            var allowUserCerdential = new AllowUserCredential()
            {
                Credentials = userWebAuthnInfos.Select(c => new Credential()
                {
                    Id = Convert.ToBase64String(c.CredentialId),
                    Type = "public-key"
                }).ToArray(),
                UserId = userObjectId.ToString(),
            };
            return Task.FromResult(allowUserCerdential);
        }
    }
}
