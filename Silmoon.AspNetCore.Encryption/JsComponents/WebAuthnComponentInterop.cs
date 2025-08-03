using Microsoft.JSInterop;
using Silmoon.AspNetCore.Encryption.ClientModels;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.Extension;
using Silmoon.Models;
using Silmoon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Encryption.JsComponents
{
    public class WebAuthnComponentInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        public WebAuthnComponentInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Silmoon.AspNetCore.Encryption/js/webAuthnComponentInterop.js").AsTask());
        }

        public async ValueTask<StateSet<bool, WebAuthnCreateResponse>> Create(ClientBlazorWebAuthnOptions options)
        {
            var module = await moduleTask.Value;
            var response = await module.InvokeAsync<StateSet<bool, WebAuthnCreateResponse>>("createWebAuthn", options);

            if (response.State && response.Data is not null)
            {
                if (response.Data.Response.Challenge == options.Challenge)
                {
                    response.Data.AttestationObjectData = WebAuthnParser.ParseAttestationObject(response.Data.Response.AttestationObject);
                    response.Data.WebAuthnInfo = WebAuthnInfo.Create(response.Data);
                    return response;
                }
                else
                {
                    response.State = false;
                    response.Message = "Challenge failed";
                    return response;
                }
            }
            else
                return response;
        }
        public async ValueTask<StateSet<bool, WebAuthnAuthenticateResponse>> Authenticate(ClientBlazorWebAuthnAuthenticateOptions options)
        {
            var module = await moduleTask.Value;
            var response = await module.InvokeAsync<StateSet<bool, WebAuthnAuthenticateResponse>>("authenticateWebAuthn", options);
            if (response.State && response.Data is not null)
            {
                if (response.Data.Response.Challenge != Convert.FromBase64String(options.Challenge).GetString())
                    return response.Set(false, "Failed(challenge is not match)!");
                else return response;
            }
            else return response;

        }
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (moduleTask.IsValueCreated)
                {
                    var module = await moduleTask.Value;
                    await module.DisposeAsync();
                }
            }
            catch { }
        }
    }
}
