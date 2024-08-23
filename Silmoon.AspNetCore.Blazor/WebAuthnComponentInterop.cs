using Microsoft.JSInterop;
using Silmoon.AspNetCore.Encryption;
using Silmoon.AspNetCore.Encryption.ClientModels;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.Models;
using Silmoon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Blazor
{
    public class WebAuthnComponentInterop
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        private DotNetObjectReference<JsInvokeObjectHandlerDelegate<Task<StateFlag<ClientBlazorWebAuthnOptions>>>> getCreateWebAuthnOptionsCallbackDotNetObjectRef;
        private DotNetObjectReference<JsInvokeCreateWebAuthnHandlerDelegate> createWebAuthnCallbackDotNetObjectRef;

        private DotNetObjectReference<JsInvokeObjectHandlerDelegate<Task<StateFlag<ClientBlazorWebAuthnAuthenticateOptions>>>> getAuthenticateWebAuthnOptionsCallbackDotNetObjectRef;
        private DotNetObjectReference<JsVoidInvokeObjectHandlerDelegate<WebAuthnAuthenticateResponse>> authenticateWebAuthnCallbackDotNetObjectRef;

        public WebAuthnComponentInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Silmoon.AspNetCore.Blazor/js/webAuthnComponentInterop.js").AsTask());
        }

        public async ValueTask Create(Func<Task<StateFlag<ClientBlazorWebAuthnOptions>>> callback, Action<BlazorWebAuthnCreateResponse> createCallback)
        {
            var module = await moduleTask.Value;

            getCreateWebAuthnOptionsCallbackDotNetObjectRef?.Dispose();
            createWebAuthnCallbackDotNetObjectRef?.Dispose();

            getCreateWebAuthnOptionsCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsInvokeObjectHandlerDelegate<Task<StateFlag<ClientBlazorWebAuthnOptions>>>(callback));
            createWebAuthnCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsInvokeCreateWebAuthnHandlerDelegate(createCallback));
            await module.InvokeVoidAsync("createWebAuthn", getCreateWebAuthnOptionsCallbackDotNetObjectRef, createWebAuthnCallbackDotNetObjectRef);
        }
        public async ValueTask Authenticate(Func<Task<StateFlag<ClientBlazorWebAuthnAuthenticateOptions>>> callback, Action<WebAuthnAuthenticateResponse> authenticateCallback)
        {
            var module = await moduleTask.Value;

            getAuthenticateWebAuthnOptionsCallbackDotNetObjectRef?.Dispose();
            authenticateWebAuthnCallbackDotNetObjectRef?.Dispose();

            getAuthenticateWebAuthnOptionsCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsInvokeObjectHandlerDelegate<Task<StateFlag<ClientBlazorWebAuthnAuthenticateOptions>>>(callback));
            authenticateWebAuthnCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsVoidInvokeObjectHandlerDelegate<WebAuthnAuthenticateResponse>(authenticateCallback));
            await module.InvokeVoidAsync("authenticateWebAuthn", getAuthenticateWebAuthnOptionsCallbackDotNetObjectRef, authenticateWebAuthnCallbackDotNetObjectRef);
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
            getCreateWebAuthnOptionsCallbackDotNetObjectRef?.Dispose();
            createWebAuthnCallbackDotNetObjectRef = null;
            getAuthenticateWebAuthnOptionsCallbackDotNetObjectRef?.Dispose();
            getAuthenticateWebAuthnOptionsCallbackDotNetObjectRef = null;
            authenticateWebAuthnCallbackDotNetObjectRef?.Dispose();
            authenticateWebAuthnCallbackDotNetObjectRef = null;
        }
    }
    public class JsInvokeCreateWebAuthnHandlerDelegate(Action<BlazorWebAuthnCreateResponse> callback)
    {
        [JSInvokable]
        public void InvokeCallback(WebAuthnCreateResponse data)
        {
            BlazorWebAuthnCreateResponse response = Copy.New<BlazorWebAuthnCreateResponse>(data);
            response.AttestationObjectData = WebAuthnParser.ParseAttestationObject(response.Response.AttestationObject);
            callback.Invoke(response);
        }
    }
    public class JsInvokeAuthenticateWebAuthnHandlerDelegate(Func<WebAuthnAuthenticateResponse, Task<StateFlag>> callback)
    {
        [JSInvokable]
        public Task<StateFlag> InvokeCallback(WebAuthnAuthenticateResponse data)
        {
            return callback != null ? callback.Invoke(data) : default;
        }
    }

}
