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
    public class WebAuthnComponentInterop
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        private DotNetObjectReference<JsInvokeCreateWebAuthnHandlerDelegate> createWebAuthnCallbackDotNetObjectRef;
        private DotNetObjectReference<JsInvokeAuthenticateWebAuthnHandlerDelegate> authenticateWebAuthnCallbackDotNetObjectRef;

        public WebAuthnComponentInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Silmoon.AspNetCore.Encryption/js/webAuthnComponentInterop.js").AsTask());
        }

        public async ValueTask Create(ClientBlazorWebAuthnOptions options, Func<StateSet<bool, BlazorWebAuthnCreateResponse>, Task> createCallback)
        {
            var module = await moduleTask.Value;
            createWebAuthnCallbackDotNetObjectRef?.Dispose();
            createWebAuthnCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsInvokeCreateWebAuthnHandlerDelegate(createCallback));
            await module.InvokeVoidAsync("createWebAuthn", options, createWebAuthnCallbackDotNetObjectRef);
        }
        public async ValueTask Authenticate(ClientBlazorWebAuthnAuthenticateOptions options, Func<StateSet<bool, WebAuthnAuthenticateResponse>, Task> authenticateCallback)
        {
            var module = await moduleTask.Value;
            authenticateWebAuthnCallbackDotNetObjectRef?.Dispose();
            authenticateWebAuthnCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsInvokeAuthenticateWebAuthnHandlerDelegate(authenticateCallback));
            await module.InvokeVoidAsync("authenticateWebAuthn", options, authenticateWebAuthnCallbackDotNetObjectRef);
        }
        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
            createWebAuthnCallbackDotNetObjectRef?.Dispose();
            createWebAuthnCallbackDotNetObjectRef = null;
            authenticateWebAuthnCallbackDotNetObjectRef?.Dispose();
            authenticateWebAuthnCallbackDotNetObjectRef = null;
        }
    }
    public class JsInvokeCreateWebAuthnHandlerDelegate(Func<StateSet<bool, BlazorWebAuthnCreateResponse>, Task> callback)
    {
        [JSInvokable]
        public Task InvokeCallback(StateSet<bool, WebAuthnCreateResponse> data)
        {
            var result = StateSet<bool, BlazorWebAuthnCreateResponse>.Create(data.State, null, data.Message);

            if (data.State && data.Data is not null)
            {
                result.Data = Copy.New<BlazorWebAuthnCreateResponse>(data.Data);
                result.Data.AttestationObjectData = WebAuthnParser.ParseAttestationObject(result.Data.Response.AttestationObject);
                return callback?.Invoke(result);
            }
            else
            {
                return callback?.Invoke(result);
            }
        }
    }
    public class JsInvokeAuthenticateWebAuthnHandlerDelegate(Func<StateSet<bool, WebAuthnAuthenticateResponse>, Task> callback)
    {
        [JSInvokable]
        public Task InvokeCallback(StateSet<bool, WebAuthnAuthenticateResponse> data) => callback?.Invoke(data);
    }
}
