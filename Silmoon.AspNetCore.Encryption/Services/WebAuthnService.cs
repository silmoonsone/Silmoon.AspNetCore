﻿using Microsoft.AspNetCore.Http;
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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;
using System.Text;

namespace Silmoon.AspNetCore.Encryption.Services
{
    public abstract class WebAuthnService : IWebAuthnService
    {
        public WebAuthnServiceOptions Options { get; set; }
        public WebAuthnService(IOptions<WebAuthnServiceOptions> options) => Options = options.Value is null ? new WebAuthnServiceOptions() : options.Value;
        public async Task GetWebAuthnOptions(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            var user = await GetClientOptionsWebAuthnUser(httpContext);
            StateFlag<ClientWebAuthnOptions> result = new StateFlag<ClientWebAuthnOptions>();

            if (user is null)
            {
                result.Success = false;
                result.Message = "User not found";
            }
            else
            {
                result.Success = true;
                result.Data = new ClientWebAuthnOptions()
                {
                    Challenge = Guid.NewGuid().ToByteArray(),
                    Rp = new ClientWebAuthnOptions.ClientWebAuthnRp() { Id = Options.Host, Name = Options.AppName },
                    User = user,
                    AuthenticatorSelection = new ClientWebAuthnOptions.ClientWebAuthnAuthenticatorSelection() { UserVerification = "preferred" },
                    Timeout = 60000
                };
                ObjectCache<string, string>.Set("______passkey_challenge:" + user.Id, Convert.ToBase64String(result.Data.Challenge), TimeSpan.FromSeconds(300));
            }
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(result.ToJsonString());
        }
        public async Task GetWebAuthnAssertionOptions(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            StateFlag<ClientWebAuthnAssertionOptions> result = new StateFlag<ClientWebAuthnAssertionOptions>();

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
                result.Data = new ClientWebAuthnAssertionOptions
                {
                    Challenge = challenge,
                    RpId = Options.Host,
                    AllowCredentials = allowUserCredential.Credentials,
                };
                ObjectCache<string, string>.Set("______passkey_challenge:" + challenge.GetBase64String(), allowUserCredential.UserId, TimeSpan.FromSeconds(300));
            }
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(result.ToJsonString());
        }
        public async Task CreateWebAuthn(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            var bodyStr = await httpContext.Request.GetBodyString();
            CreateWebAuthnKeyResponse createWebAuthnKeyResponse = JsonConvert.DeserializeObject<CreateWebAuthnKeyResponse>(bodyStr);

            var attestationObjectByteArray = createWebAuthnKeyResponse.Response.AttestationObject;
            var clientDataJSON = createWebAuthnKeyResponse.Response.ClientDataJson;
            var attestationData = WebAuthnParser.ParseAttestationObject(attestationObjectByteArray);

            var createResult = await OnCreateWebAuthn(httpContext, attestationData, clientDataJSON.GetString(), attestationObjectByteArray, createWebAuthnKeyResponse.AuthenticatorAttachment);

            StateFlag<bool> result = new StateFlag<bool>();
            if (createResult.State)
            {
                result.Success = true;
            }
            else
            {
                result.Success = false;
                result.Message = createResult.Message;
            }
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(result.ToJsonString());
        }
        public async Task DeleteWebAuthn(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            string credentialId = httpContext.Request.Form["CredentialId"];
            if (credentialId.IsNullOrEmpty()) credentialId = httpContext.Request.Query["CredentialId"];


            StateFlag result = new StateFlag();

            if (credentialId.IsNullOrEmpty())
            {
                result.Success = false;
                result.Message = "CredentialId is empty";
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(result.ToJsonString());
            }
            else
            {
                var deleteResult = await OnDeleteWebAuthn(httpContext, Convert.FromBase64String(credentialId));
                if (deleteResult.State)
                {
                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.Message = deleteResult.Message;
                }
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(result.ToJsonString());

            }
        }
        public async Task VerifyWebAuthn(HttpContext httpContext, RequestDelegate requestDelegate)
        {
            var bodyStr = await httpContext.Request.GetBodyString();
            VerifyWebAuthnResponse verifyWebAuthnResponse = JsonConvert.DeserializeObject<VerifyWebAuthnResponse>(bodyStr);

            byte[] rawId = verifyWebAuthnResponse.RawId;
            byte[] clientDataJSON = verifyWebAuthnResponse.Response.ClientDataJSON;
            byte[] authenticatorData = verifyWebAuthnResponse.Response.AuthenticatorData;
            byte[] signature = verifyWebAuthnResponse.Response.Signature;

            var clientDataString = Encoding.UTF8.GetString(clientDataJSON);
            var clientData = JsonConvert.DeserializeObject<JObject>(clientDataString);

            var userIdResult = ObjectCache<string, string>.Get("______passkey_challenge:" + clientData["challenge"].Value<string>().Base64UrlToBase64());

            StateFlag result = new StateFlag();

            if (!userIdResult.Matched)
            {
                result.Success = false;
                result.Message = "Challenge not found";
            }
            else
            {
                var publicKeyInfo = await OnGetPublicKeyInfo(httpContext, rawId, userIdResult.Value);
                if (publicKeyInfo is null)
                {
                    result.Success = false;
                    result.Message = "Credential not found";
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(result.ToJsonString());
                }

                // 组合签名数据：authenticatorData + SHA256(clientDataJSON)
                var signedData = authenticatorData.Concat(SHA256.HashData(clientDataJSON)).ToArray();

                // 验证签名
                bool isValidSignature = false;
                if (publicKeyInfo.PublicKeyAlgorithm == "ES256")
                {
                    byte[] publicKey = publicKeyInfo.PublicKey;  // 直接使用提取和存储的公钥
                    using var ecdsa = ECDsa.Create();
                    ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
                    isValidSignature = ecdsa.VerifyData(signedData, WebAuthnParser.ConvertDerToRS(signature), HashAlgorithmName.SHA256);
                }
                else if (publicKeyInfo.PublicKeyAlgorithm == "RS256")
                {
                    var rsa = RSA.Create();
                    rsa.ImportRSAPublicKey(publicKeyInfo.PublicKey, out _);
                    isValidSignature = rsa.VerifyData(signedData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
                else
                {
                    result.Success = false;
                    result.Message = "Unsupported public key algorithm";
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(result.ToJsonString());
                    return;
                }

                if (!isValidSignature)
                {
                    result.Success = false;
                    result.Message = "Invalid signature";
                }
                else
                    result.Success = true;
            }
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(result.ToJsonString());
        }


        public abstract Task<ClientWebAuthnOptions.ClientWebAuthnUser> GetClientOptionsWebAuthnUser(HttpContext httpContext);
        public abstract Task<AllowUserCredential> GetAllowCredentials(HttpContext httpContext, string userId);
        public abstract Task<StateSet<bool>> OnCreateWebAuthn(HttpContext httpContext, AttestationObjectData attestationObjectData, string clientDataJSON, byte[] attestationObjectByteArray, string authenticatorAttachment);
        public abstract Task<StateSet<bool>> OnDeleteWebAuthn(HttpContext httpContext, byte[] credentialId);
        public abstract Task<PublicKeyInfo> OnGetPublicKeyInfo(HttpContext httpContext, byte[] rawId, string userId);
    }
}