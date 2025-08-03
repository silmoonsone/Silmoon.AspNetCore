using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Encryption.ClientModels;
using Silmoon.AspNetCore.Encryption.JsComponents;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.AspNetCore.Encryption.Services.Interfaces;
using Silmoon.AspNetCore.Extensions;
using Silmoon.AspNetCore.Interfaces;
using Silmoon.Extension;
using Silmoon.Extension.Models;
using Silmoon.Models;
using Silmoon.Runtime;
using Silmoon.Runtime.Cache;
using Silmoon.Secure;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Encryption.Services
{
    public abstract class BlazorWebAuthnService
    {
        WebAuthnComponentInterop WebAuthnInterop { get; set; }
        IHttpContextAccessor HttpContextAccessor { get; set; }
        public BlazorWebAuthnService(IHttpContextAccessor httpContextAccessor, WebAuthnComponentInterop webAuthnComponentInterop)
        {
            HttpContextAccessor = httpContextAccessor;
            WebAuthnInterop = webAuthnComponentInterop;
        }
        public async Task<StateSet<bool, WebAuthnCreateResponse>> Create(string name, string displayName, byte[] id, string challenge = null)
        {
            challenge ??= HashHelper.RandomChars(32);
            ClientBlazorWebAuthnOptions clientBlazorWebAuthnOptions = new ClientBlazorWebAuthnOptions()
            {
                Challenge = challenge,
                Rp = new ClientWebAuthnOptions.ClientWebAuthnRp()
                {
                    Id = null,
                    Name = "LocalhostTest"
                },
                User = new ClientBlazorWebAuthnOptions.ClientWebAuthnUser()
                {
                    Name = name,
                    DisplayName = displayName,
                    Id = id.GetBase64String(),
                },
            };
            var response = await WebAuthnInterop.Create(clientBlazorWebAuthnOptions);
            return response;
        }
        public async Task<StateSet<bool, WebAuthnAuthenticateResponse>> Request(Credential[] credentials, string challenge = null)
        {
            challenge ??= HashHelper.RandomChars(32);
            challenge = challenge.ToBase64String();

            ClientBlazorWebAuthnAuthenticateOptions options = new ClientBlazorWebAuthnAuthenticateOptions()
            {
                Challenge = challenge,
                RpId = null,
                AllowCredentials = credentials,
            };
            var webAuthnAuthenticateResponse = await WebAuthnInterop.Authenticate(options);
            return webAuthnAuthenticateResponse;
        }
        public async Task<StateSet<bool>> Authenticate(string userId, string challenge = null)
        {
            var credentials = await GetAllowCredentials(HttpContextAccessor.HttpContext, userId);
            var response = await Request(credentials, challenge);
            if (response.State)
            {
                if (challenge is null || response.Data.Response.Challenge == challenge)
                {
                    var publicKeyInfo = await OnGetPublicKeyInfo(HttpContextAccessor.HttpContext, response.Data.RawId, userId);
                    if (publicKeyInfo is null) return false.ToStateSet("Failed(credential is not exist)!");
                    else
                    {
                        var result = response.Data.VerifySignature(publicKeyInfo);
                        return result.State.ToStateSet(result.Message);
                    }
                }
                return false.ToStateSet("Failed(challenge is not match)!");
            }
            else
            {
                return false.ToStateSet(response.Message);
            }
        }


        /// <summary>
        /// 用户启动WebAuthn验证时获取客户端配置，根据userId获取用户的WebAuthn信息，或者userId为空时不指定信息
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public abstract Task<Credential[]> GetAllowCredentials(HttpContext httpContext, string userId);
        /// <summary>
        /// 执行验证过程中获取公钥信息
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="rawId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public abstract Task<PublicKeyInfo> OnGetPublicKeyInfo(HttpContext httpContext, byte[] rawId, string userId = null);
    }
}
