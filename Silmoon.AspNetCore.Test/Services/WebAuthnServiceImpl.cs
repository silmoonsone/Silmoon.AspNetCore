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
        ISilmoonAuthService SilmoonAuthService { get; set; }
        public WebAuthnServiceImpl(IOptions<WebAuthnServiceOptions> options, Core core) : base(options)
        {
            Core = core;
        }

        public override async Task<ClientWebAuthnOptions.ClientWebAuthnUser> GetClientOptionsWebAuthnUser(HttpContext httpContext)
        {
            if (await SilmoonAuthService.IsSignIn())
            {
                var user = await SilmoonAuthService.GetUser<User>();
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
        public override async Task<StateSet<bool>> OnCreate(HttpContext httpContext, AttestationObjectData attestationObjectData, string clientDataJSON, byte[] attestationObjectByteArray, string authenticatorAttachment)
        {
            var user = await SilmoonAuthService.GetUser<User>();
            if (user is null) return false.ToStateSet("User not found");
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
                return result;
            }
        }
        public override async Task<StateSet<bool>> OnDelete(HttpContext httpContext, byte[] credentialId)
        {
            var user = await SilmoonAuthService.GetUser<User>();
            if (user is null) return false.ToStateSet("User not found");
            else
            {
                var result = Core.DeleteUserWebAuthnInfo(user._id, credentialId);
                if (result.State)
                    return true.ToStateSet();
                else
                    return false.ToStateSet(result.Message);
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
            if (userWebAuthnInfos is not null)
            {
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
            else return null;
        }
    }
}
