using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Encryption.ClientModels;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.AspNetCore.Encryption.Services.Interfaces;
using Silmoon.AspNetCore.Extensions;
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
    public abstract class WebAuthnService : IWebAuthnService
    {
        public WebAuthnServiceOptions Options { get; set; }
        public WebAuthnService(IOptions<WebAuthnServiceOptions> options) => Options = options.Value;
        public async Task GetCreateOptions(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            var getResult = await GetClientCreateWebAuthnOptions(httpContext);
            StateResult<ClientWebAuthnOptions> result = new StateResult<ClientWebAuthnOptions>();

            if (getResult.State)
            {
                result.Success = true;
                result.Data = new ClientWebAuthnOptions()
                {
                    Challenge = Guid.NewGuid().ToByteArray(),
                    Rp = new ClientWebAuthnOptions.ClientWebAuthnRp() { Id = Options.Host, Name = Options.AppName },
                    User = getResult.Data,
                    AuthenticatorSelection = new ClientWebAuthnOptions.ClientWebAuthnAuthenticatorSelection() { UserVerification = "preferred" },
                    Timeout = 60000
                };
                GlobalCaching<string, string>.Set("_passkey_challenge:" + result.Data.Challenge.GetBase64String(), getResult.Data.Id.GetBase64String(), TimeSpan.FromSeconds(300));
            }
            else result.Set(false, getResult.Message);
            await httpContext.Response.WriteJObjectAsync(result);
        }
        public async Task GetAuthenticateOptions(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            StateResult<ClientWebAuthnAuthenticateOptions> result = new StateResult<ClientWebAuthnAuthenticateOptions>();

            string userId = httpContext.Request.Form["UserId"];
            string challengeStr = httpContext.Request.Form["Challenge"];
            byte[] challenge;

            if (challengeStr.IsNullOrEmpty()) challenge = HashHelper.RandomChars(32).GetBytes();
            else
            {
                if (challengeStr.Length > 1024)
                {
                    result.Set(false, "Challenge must be less than 1024 characters.");
                    goto fin;
                }
                else challenge = challengeStr.GetBytes();
            }

            var allowUserCredential = await GetAllowCredentials(httpContext, userId);

            if (allowUserCredential is null) result.Set(false, "User not found");
            else
            {
                result.Success = true;
                result.Data = new ClientWebAuthnAuthenticateOptions
                {
                    Challenge = challenge,
                    RpId = Options.Host,
                    AllowCredentials = allowUserCredential.Credentials,
                };

                if (challengeStr.IsNullOrEmpty()) GlobalCaching<string, string>.Set("_passkey_challenge:" + challenge.GetBase64String(), allowUserCredential.UserId, TimeSpan.FromSeconds(300));
                else GlobalCaching<string, string>.Set("__passkey_challenge:" + challenge.GetBase64String(), allowUserCredential.UserId, TimeSpan.FromSeconds(300));
            }
            fin:
            await httpContext.Response.WriteJObjectAsync(result);
        }
        public async Task Create(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            var bodyStr = await httpContext.Request.GetBodyString();
            WebAuthnCreateResponse createWebAuthnKeyResponse = JsonConvert.DeserializeObject<WebAuthnCreateResponse>(bodyStr);
            var clientDataJSON = createWebAuthnKeyResponse.Response.ClientDataJson.GetString();
            var clientJson = JObject.Parse(clientDataJSON);

            var (isFindUserId, findedUserId) = GlobalCaching<string, string>.Get("_passkey_challenge:" + clientJson["challenge"].Value<string>().Base64UrlToBase64());

            StateResult<bool> result = new StateResult<bool>();
            if (!isFindUserId) result.Set(false, "Challenge failed");
            else
            {
                createWebAuthnKeyResponse.AttestationObjectData = WebAuthnParser.ParseAttestationObject(createWebAuthnKeyResponse.Response.AttestationObject);
                createWebAuthnKeyResponse.WebAuthnInfo = WebAuthnInfo.Create(createWebAuthnKeyResponse);

                var createResult = await OnCreate(httpContext, createWebAuthnKeyResponse);
                result.Set(createResult.State, createResult.Message);
            }
            await httpContext.Response.WriteJObjectAsync(result);
        }
        public async Task Delete(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            string credentialId = httpContext.Request.Form["CredentialId"];
            if (credentialId.IsNullOrEmpty()) credentialId = httpContext.Request.Query["CredentialId"];


            StateResult result = new StateResult();

            if (credentialId.IsNullOrEmpty()) result.Set(false, "CredentialId is empty");
            else
            {
                var deleteResult = await OnDelete(httpContext, Convert.FromBase64String(credentialId));
                if (deleteResult.State) result.Set(true, deleteResult.Message);
                else result.Set(false, deleteResult.Message);
            }
            await httpContext.Response.WriteJObjectAsync(result);
        }
        public async Task Authenticate(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            var bodyStr = await httpContext.Request.GetBodyString();
            WebAuthnAuthenticateResponse verifyWebAuthnResponse = JsonConvert.DeserializeObject<WebAuthnAuthenticateResponse>(bodyStr);

            byte[] rawId = verifyWebAuthnResponse.RawId;
            //byte[] clientDataJSON = verifyWebAuthnResponse.Response.ClientDataJson;
            //byte[] authenticatorData = verifyWebAuthnResponse.Response.AuthenticatorData;
            //byte[] signature = verifyWebAuthnResponse.Response.Signature;

            var clientData = verifyWebAuthnResponse.Response.GetClientJson();

            var userIdResult = GlobalCaching<string, string>.Get("_passkey_challenge:" + clientData["challenge"].Value<string>().Base64UrlToBase64());
            GlobalCaching<string, string>.Remove("_passkey_challenge:" + clientData["challenge"].Value<string>().Base64UrlToBase64());

            StateSet<bool> result = new StateSet<bool>();
            if (!userIdResult.Matched) result.Set(false, "Challenge failed");
            else
            {
                var publicKeyInfo = await OnGetPublicKeyInfo(httpContext, rawId, userIdResult.Value);
                if (publicKeyInfo is null) result.Set(false, "Credential not found");
                else
                {
                    // 组合签名数据：authenticatorData + SHA256(clientDataJSON)
                    var signedData = verifyWebAuthnResponse.SignedData;
                    // 验证签名
                    var validSignatureResult = verifyWebAuthnResponse.VerifySignature(publicKeyInfo);
                    result.Set(validSignatureResult.State, validSignatureResult.Message);
                }
            }
            var newResult = await OnAuthenticateCompleted(httpContext, verifyWebAuthnResponse, result, verifyWebAuthnResponse.FlagData);
            StateResult<object> stateResult = new StateResult<object>() { Success = newResult.State, Data = verifyWebAuthnResponse.FlagData, Message = newResult.Message };
            await httpContext.Response.WriteJObjectAsync(stateResult);
        }

        public async Task<StateSet<bool>> ValidateData(HttpContext httpContext, string jsonStringData, string challenge = null)
        {
            challenge = challenge?.GetBytes().GetBase64String();
            WebAuthnAuthenticateResponse verifyWebAuthnResponse = JsonConvert.DeserializeObject<WebAuthnAuthenticateResponse>(jsonStringData);

            byte[] rawId = verifyWebAuthnResponse.RawId;
            var clientData = verifyWebAuthnResponse.Response.GetClientJson();
            var clientChallengeData = clientData["challenge"].Value<string>().Base64UrlToBase64();
            //byte[] clientDataJSON = verifyWebAuthnResponse.Response.ClientDataJson;
            //byte[] authenticatorData = verifyWebAuthnResponse.Response.AuthenticatorData;
            //byte[] signature = verifyWebAuthnResponse.Response.Signature;

            var cacheKey = challenge.IsNullOrEmpty() ? "_passkey_challenge:" + clientChallengeData : "__passkey_challenge:" + clientChallengeData;
            var (isFindUserId, findedUserId) = GlobalCaching<string, string>.Get(cacheKey);
            GlobalCaching<string, string>.Remove(cacheKey);

            if (!challenge.IsNullOrEmpty() && clientChallengeData != challenge)
            {
                return false.ToStateSet("Signature challenge not match.");
            }
            else
            {
                if (!isFindUserId)
                    return false.ToStateSet("Challenge failed");
                else
                {
                    var publicKeyInfo = await OnGetPublicKeyInfo(httpContext, rawId, findedUserId);
                    if (publicKeyInfo is null)
                        return false.ToStateSet("Credential not found");
                    else
                    {
                        // 组合签名数据：authenticatorData + SHA256(clientDataJSON)
                        var signedData = verifyWebAuthnResponse.SignedData;
                        // 验证签名
                        var validSignatureResult = verifyWebAuthnResponse.VerifySignature(publicKeyInfo);
                        return validSignatureResult;
                    }
                }
            }
        }

        /// <summary>
        /// 用户创建WebAuthn时获取客户端配置
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public abstract Task<StateSet<bool, ClientWebAuthnOptions.ClientWebAuthnUser>> GetClientCreateWebAuthnOptions(HttpContext httpContext);
        /// <summary>
        /// 用户启动WebAuthn验证时获取客户端配置，根据userId获取用户的WebAuthn信息，或者userId为空时不指定信息
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public abstract Task<AllowUserCredential> GetAllowCredentials(HttpContext httpContext, string userId);
        /// <summary>
        /// 当用户浏览器已经创建WebAuthn后，服务器端执行保存操作
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="webAuthnCreateResponse"></param>
        /// <returns></returns>
        public abstract Task<StateSet<bool>> OnCreate(HttpContext httpContext, WebAuthnCreateResponse webAuthnCreateResponse);
        /// <summary>
        /// 当验证过程完成后，执行的操作，通过result参数判断验证是否成功
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="webAuthnAuthenticateResponse"></param>
        /// <param name="result"></param>
        /// <param name="flagData"></param>
        /// <returns></returns>
        public abstract Task<StateSet<bool>> OnAuthenticateCompleted(HttpContext httpContext, WebAuthnAuthenticateResponse webAuthnAuthenticateResponse, StateSet<bool> result, object flagData);
        /// <summary>
        /// 删除WebAuthn
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="credentialId"></param>
        /// <returns></returns>
        public abstract Task<StateSet<bool>> OnDelete(HttpContext httpContext, byte[] credentialId);
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
