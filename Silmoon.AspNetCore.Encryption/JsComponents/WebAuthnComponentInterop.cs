using Microsoft.JSInterop;
using Silmoon.AspNetCore.Encryption.ClientModels;
using Silmoon.AspNetCore.Encryption.Models;
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
        private DotNetObjectReference<JsInvokeCreateWebAuthnHandlerDelegate> createWebAuthnCallbackDotNetObjectRef;
        private DotNetObjectReference<JsInvokeAuthenticateWebAuthnHandlerDelegate> authenticateWebAuthnCallbackDotNetObjectRef;

        public WebAuthnComponentInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Silmoon.AspNetCore.Encryption/js/webAuthnComponentInterop.js").AsTask());
        }

        public async ValueTask Create(ClientBlazorWebAuthnOptions options, Func<StateSet<bool, WebAuthnCreateResponse>, Task> createCallback)
        {
            var module = await moduleTask.Value;
            createWebAuthnCallbackDotNetObjectRef?.Dispose();
            createWebAuthnCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsInvokeCreateWebAuthnHandlerDelegate(createCallback, options.Challenge));
            await module.InvokeVoidAsync("createWebAuthn", options, createWebAuthnCallbackDotNetObjectRef);
        }
        public async ValueTask Authenticate(ClientBlazorWebAuthnAuthenticateOptions options, Func<StateSet<bool, WebAuthnAuthenticateResponse>, Task> authenticateCallback)
        {
            var module = await moduleTask.Value;
            authenticateWebAuthnCallbackDotNetObjectRef?.Dispose();
            authenticateWebAuthnCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsInvokeAuthenticateWebAuthnHandlerDelegate(authenticateCallback, options.Challenge));
            await module.InvokeVoidAsync("authenticateWebAuthn", options, authenticateWebAuthnCallbackDotNetObjectRef);
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

            try
            {
                createWebAuthnCallbackDotNetObjectRef?.Dispose();
                createWebAuthnCallbackDotNetObjectRef = null;
            }
            catch { }
            try
            {
                authenticateWebAuthnCallbackDotNetObjectRef?.Dispose();
                authenticateWebAuthnCallbackDotNetObjectRef = null;
            }
            catch { }
        }
    }
    public class JsInvokeCreateWebAuthnHandlerDelegate(Func<StateSet<bool, WebAuthnCreateResponse>, Task> callback, string challenge)
    {
        [JSInvokable]
        public Task InvokeCallback(StateSet<bool, WebAuthnCreateResponse> response)
        {
            var result = StateSet<bool, WebAuthnCreateResponse>.Create(response.State, null, response.Message);

            if (response.State && response.Data is not null)
            {
                if (response.Data.Response.Challenge == challenge)
                {
                    result.Data = Copy.New<WebAuthnCreateResponse>(response.Data);
                    result.Data.AttestationObjectData = WebAuthnParser.ParseAttestationObject(response.Data.Response.AttestationObject);
                    result.Data.WebAuthnInfo = WebAuthnInfo.Create(response.Data);
                    return callback?.Invoke(result);
                }
                else
                {
                    result.State = false;
                    result.Message = "Challenge failed";
                    return callback?.Invoke(result);
                }
            }
            else
                return callback?.Invoke(result);
        }
    }
    public class JsInvokeAuthenticateWebAuthnHandlerDelegate(Func<StateSet<bool, WebAuthnAuthenticateResponse>, Task> callback, string challenge)
    {
        [JSInvokable]
        public Task InvokeCallback(StateSet<bool, WebAuthnAuthenticateResponse> response)
        {
            if (response.State && response.Data is not null)
            {
                if (response.Data.Response.Challenge == challenge)
                {
                    return callback?.Invoke(response);
                }
                else
                {
                    response.State = false;
                    response.Message = "Challenge failed";
                    return callback?.Invoke(response);
                }
            }
            else
                return callback?.Invoke(response);
        }
    }
}
