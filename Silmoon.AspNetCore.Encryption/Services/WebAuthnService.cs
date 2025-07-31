using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Encryption.ClientModels;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.AspNetCore.Encryption.Services.Interfaces;
using Silmoon.AspNetCore.Extensions;
using Silmoon.Extension;
using Silmoon.Models;
using Silmoon.Runtime;
using Silmoon.Runtime.Cache;
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
            StateFlag<ClientWebAuthnOptions> result = new StateFlag<ClientWebAuthnOptions>();

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
                GlobalCaching<string, string>.Set("______passkey_challenge:" + result.Data.Challenge.GetBase64String(), getResult.Data.Id.GetBase64String(), TimeSpan.FromSeconds(300));
            }
            else
            {
                result.Success = false;
                result.Message = getResult.Message;
            }
            await httpContext.Response.WriteJObjectAsync(result);
        }
        public async Task GetAuthenticateOptions(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            StateFlag<ClientWebAuthnAuthenticateOptions> result = new StateFlag<ClientWebAuthnAuthenticateOptions>();

            var challenge = Guid.NewGuid().ToByteArray();
            string userId = httpContext.Request.Query["UserId"];
            var allowUserCredential = await GetAllowCredentials(httpContext, userId);

            if (allowUserCredential is null)
            {
                result.Success = false;
                result.Message = "User not found";
            }
            else
            {
                result.Success = true;
                result.Data = new ClientWebAuthnAuthenticateOptions
                {
                    Challenge = challenge,
                    RpId = Options.Host,
                    AllowCredentials = allowUserCredential.Credentials,
                };
                GlobalCaching<string, string>.Set("______passkey_challenge:" + challenge.GetBase64String(), allowUserCredential.UserId, TimeSpan.FromSeconds(300));
            }
            await httpContext.Response.WriteJObjectAsync(result);
        }
        public async Task Create(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            var bodyStr = await httpContext.Request.GetBodyString();
            WebAuthnCreateResponse createWebAuthnKeyResponse = JsonConvert.DeserializeObject<WebAuthnCreateResponse>(bodyStr);
            var clientDataJSON = createWebAuthnKeyResponse.Response.ClientDataJson.GetString();
            var clientJson = JObject.Parse(clientDataJSON);

            var userIdResult = GlobalCaching<string, string>.Get("______passkey_challenge:" + clientJson["challenge"].Value<string>().Base64UrlToBase64());

            StateFlag<bool> result = new StateFlag<bool>();
            if (!userIdResult.Matched)
            {
                result.Success = false;
                result.Message = "Challenge failed";
            }
            else
            {
                createWebAuthnKeyResponse.AttestationObjectData = WebAuthnParser.ParseAttestationObject(createWebAuthnKeyResponse.Response.AttestationObject);
                createWebAuthnKeyResponse.WebAuthnInfo = WebAuthnInfo.Create(createWebAuthnKeyResponse);

                var createResult = await OnCreate(httpContext, createWebAuthnKeyResponse);

                if (createResult.State)
                {
                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.Message = createResult.Message;
                }
            }
            await httpContext.Response.WriteJObjectAsync(result);
        }
        public async Task Delete(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            string credentialId = httpContext.Request.Form["CredentialId"];
            if (credentialId.IsNullOrEmpty()) credentialId = httpContext.Request.Query["CredentialId"];


            StateFlag result = new StateFlag();

            if (credentialId.IsNullOrEmpty())
            {
                result.Success = false;
                result.Message = "CredentialId is empty";
                await httpContext.Response.WriteJObjectAsync(result);
            }
            else
            {
                var deleteResult = await OnDelete(httpContext, Convert.FromBase64String(credentialId));
                if (deleteResult.State)
                {
                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.Message = deleteResult.Message;
                }
                await httpContext.Response.WriteJObjectAsync(result);

            }
        }
        public async Task Authenticate(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            var bodyStr = await httpContext.Request.GetBodyString();
            WebAuthnAuthenticateResponse verifyWebAuthnResponse = JsonConvert.DeserializeObject<WebAuthnAuthenticateResponse>(bodyStr);

            byte[] rawId = verifyWebAuthnResponse.RawId;
            byte[] clientDataJSON = verifyWebAuthnResponse.Response.ClientDataJson;
            byte[] authenticatorData = verifyWebAuthnResponse.Response.AuthenticatorData;
            byte[] signature = verifyWebAuthnResponse.Response.Signature;

            var clientData = verifyWebAuthnResponse.Response.GetClientJson();

            var userIdResult = GlobalCaching<string, string>.Get("______passkey_challenge:" + clientData["challenge"].Value<string>().Base64UrlToBase64());
            GlobalCaching<string, string>.Remove("______passkey_challenge:" + clientData["challenge"].Value<string>().Base64UrlToBase64());

            StateSet<bool> result = new StateSet<bool>();
            if (!userIdResult.Matched)
            {
                result.State = false;
                result.Message = "Challenge failed";
            }
            else
            {
                var publicKeyInfo = await OnGetPublicKeyInfo(httpContext, rawId, userIdResult.Value);
                if (publicKeyInfo is null)
                {
                    result.State = false;
                    result.Message = "Credential not found";
                }
                else
                {
                    // 组合签名数据：authenticatorData + SHA256(clientDataJSON)
                    var signedData = verifyWebAuthnResponse.SignedData;

                    // 验证签名
                    var validSignatureResult = verifyWebAuthnResponse.VerifySignature(publicKeyInfo);

                    result.State = validSignatureResult.State;
                    result.Message = validSignatureResult.Message;
                }
            }
            var newResult = await OnAuthenticateCompleted(httpContext, verifyWebAuthnResponse, result, verifyWebAuthnResponse.FlagData);
            StateFlag<object> stateFlag = new StateFlag<object>() { Success = newResult.State, Data = verifyWebAuthnResponse.FlagData, Message = newResult.Message };
            await httpContext.Response.WriteJObjectAsync(stateFlag);
        }

        public async Task<StateSet<bool>> ValidateData(HttpContext httpContext, string jsonStringData)
        {
            var bodyStr = jsonStringData;
            WebAuthnAuthenticateResponse verifyWebAuthnResponse = JsonConvert.DeserializeObject<WebAuthnAuthenticateResponse>(bodyStr);

            byte[] rawId = verifyWebAuthnResponse.RawId;
            byte[] clientDataJSON = verifyWebAuthnResponse.Response.ClientDataJson;
            byte[] authenticatorData = verifyWebAuthnResponse.Response.AuthenticatorData;
            byte[] signature = verifyWebAuthnResponse.Response.Signature;

            var clientData = verifyWebAuthnResponse.Response.GetClientJson();

            var userIdResult = GlobalCaching<string, string>.Get("______passkey_challenge:" + clientData["challenge"].Value<string>().Base64UrlToBase64());
            GlobalCaching<string, string>.Remove("______passkey_challenge:" + clientData["challenge"].Value<string>().Base64UrlToBase64());

            StateSet<bool> result = new StateSet<bool>();
            if (!userIdResult.Matched)
            {
                result.State = false;
                result.Message = "Challenge failed";
            }
            else
            {
                var publicKeyInfo = await OnGetPublicKeyInfo(httpContext, rawId, userIdResult.Value);
                if (publicKeyInfo is null)
                {
                    result.State = false;
                    result.Message = "Credential not found";
                }
                else
                {
                    // 组合签名数据：authenticatorData + SHA256(clientDataJSON)
                    var signedData = verifyWebAuthnResponse.SignedData;

                    // 验证签名
                    var validSignatureResult = verifyWebAuthnResponse.VerifySignature(publicKeyInfo);

                    result.State = validSignatureResult.State;
                    result.Message = validSignatureResult.Message;
                }
            }
            var newResult = await OnAuthenticateCompleted(httpContext, verifyWebAuthnResponse, result, verifyWebAuthnResponse.FlagData);

            return newResult;
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
