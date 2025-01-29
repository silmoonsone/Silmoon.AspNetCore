using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Encryption.ClientModels;
using Silmoon.AspNetCore.Encryption.Services.Interfaces;
using Silmoon.Models;
using Silmoon.Runtime.Cache;
using System.Linq;
using System;
using System.Threading.Tasks;
using Silmoon.Extension;
using System.Security.Claims;
using Silmoon.AspNetCore.Extensions;
using Silmoon.AspNetCore.Encryption.Models;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Silmoon.Runtime;

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

            if (!getResult.State)
            {
                result.Success = false;
                result.Message = getResult.Message;
            }
            else
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
            StateFlag<string> stateFlag = new StateFlag<string>() { Success = newResult.State, Data = verifyWebAuthnResponse.FlagData, Message = newResult.Message };
            await httpContext.Response.WriteJObjectAsync(stateFlag);
        }


        public abstract Task<StateSet<bool, ClientWebAuthnOptions.ClientWebAuthnUser>> GetClientCreateWebAuthnOptions(HttpContext httpContext);
        public abstract Task<AllowUserCredential> GetAllowCredentials(HttpContext httpContext, string userId);
        public abstract Task<StateSet<bool>> OnCreate(HttpContext httpContext, WebAuthnCreateResponse webAuthnCreateResponse);
        public abstract Task<StateSet<bool>> OnAuthenticateCompleted(HttpContext httpContext, WebAuthnAuthenticateResponse webAuthnAuthenticateResponse, StateSet<bool> result, string flagData);
        public abstract Task<StateSet<bool>> OnDelete(HttpContext httpContext, byte[] credentialId);
        public abstract Task<PublicKeyInfo> OnGetPublicKeyInfo(HttpContext httpContext, byte[] rawId, string userId = null);
    }
}
