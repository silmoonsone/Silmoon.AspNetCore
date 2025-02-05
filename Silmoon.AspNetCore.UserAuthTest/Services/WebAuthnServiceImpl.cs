using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Silmoon.AspNetCore.Encryption.ClientModels;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.AspNetCore.Encryption.Services;
using Silmoon.AspNetCore.Services.Interfaces;
using Silmoon.AspNetCore.UserAuthTest.Models;
using Silmoon.AspNetCore.UserAuthTest.Models.SubModels;
using Silmoon.Extension;
using Silmoon.Models;
using Silmoon.Runtime;
using System.Security.Claims;

namespace Silmoon.AspNetCore.UserAuthTest.Services
{
    public class WebAuthnServiceImpl : WebAuthnService
    {
        Core Core { get; set; }
        ISilmoonAuthService SilmoonAuthService { get; set; }
        public WebAuthnServiceImpl(Core core, ISilmoonAuthService silmoonAuthService, IOptions<WebAuthnServiceOptions> options) : base(options)
        {
            Core = core;
            SilmoonAuthService = silmoonAuthService;
        }

        public override Task<AllowUserCredential> GetAllowCredentials(HttpContext httpContext, string userId)
        {
            var hasUserObjectId = ObjectId.TryParse(userId, out var userObjectId);
            var allowUserCerdential = new AllowUserCredential();
            if (hasUserObjectId)
            {
                var userWebAuthnInfos = Core.GetUserWebAuthnInfos(userObjectId);
                if (!userWebAuthnInfos.IsNullOrEmpty())
                {
                    allowUserCerdential.UserId = userObjectId.ToString();
                    allowUserCerdential.Credentials = userWebAuthnInfos.Select(c => new Credential()
                    {
                        Id = Convert.ToBase64String(c.CredentialId),
                        Type = "public-key"
                    }).ToArray();
                }
            }
            return Task.FromResult(allowUserCerdential);
        }
        public override async Task<StateSet<bool, ClientWebAuthnOptions.ClientWebAuthnUser>> GetClientCreateWebAuthnOptions(HttpContext httpContext)
        {
            var user = await SilmoonAuthService.GetUser<User>();

            var result = new ClientWebAuthnOptions.ClientWebAuthnUser()
            {
                DisplayName = "Silmoon.AspNetCore.UserAuthTest",
                Id = user._id.ToByteArray(),
                Name = user.Username,
            };
            return true.ToStateSet(result);
        }
        public override async Task<StateSet<bool>> OnCreate(HttpContext httpContext, WebAuthnCreateResponse webAuthnCreateResponse)
        {
            var user = await SilmoonAuthService.GetUser<User>();
            if (user is null) return false.ToStateSet("User not found");
            else
            {
                var wsebAuthInfo = webAuthnCreateResponse.WebAuthnInfo;
                var userWebAuthInfo = Copy.New<WebAuthnInfo, UserWebAuthnInfo>(wsebAuthInfo);
                userWebAuthInfo.UserObjectId = user._id;
                Core.AddUserWebAuthnInfo(userWebAuthInfo);
                return true.ToStateSet();
            }
        }
        public override Task<StateSet<bool>> OnAuthenticateCompleted(HttpContext httpContext, WebAuthnAuthenticateResponse webAuthnAuthenticateResponse, StateSet<bool> result, object flagData)
        {
            return Task.FromResult(result);
        }
        public override async Task<StateSet<bool>> OnDelete(HttpContext httpContext, byte[] credentialId)
        {
            var user = await SilmoonAuthService.GetUser<User>();
            if (user is null) return false.ToStateSet("User not found");
            else
            {
                var result = Core.DeleteUserWebAuthnInfo(user._id, credentialId);
                if (result == 0)
                    return false.ToStateSet("No credential be delete");
                else
                    return true.ToStateSet();
            }
        }
        public override Task<PublicKeyInfo> OnGetPublicKeyInfo(HttpContext httpContext, byte[] rawId, string userId = null)
        {
            if (userId.IsNullOrEmpty())
            {
                var credential = Core.GetUserWebAuthnInfo(rawId);
                if (credential is null)
                    return Task.FromResult<PublicKeyInfo>(null);
                else
                {
                    PublicKeyInfo publicKeyInfo = new PublicKeyInfo()
                    {
                        PublicKey = credential.PublicKey,
                        PublicKeyAlgorithm = credential.PublicKeyAlgorithm,
                    };
                    return Task.FromResult(publicKeyInfo);
                }
            }
            else
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
        }
    }
}
