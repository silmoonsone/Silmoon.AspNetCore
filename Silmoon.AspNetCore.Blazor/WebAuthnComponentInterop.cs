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
        private DotNetObjectReference<JsInvokeObjectHandlerDelegate<Task<StateFlag<BlazorClientWebAuthnOptions>>>> getCreateWebAuthnOptionsCallbackDotNetObjectRef;
        private DotNetObjectReference<JsInvokeCreateWebAuthnHandlerDelegate> createWebAuthnCallbackDotNetObjectRef;

        public WebAuthnComponentInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Silmoon.AspNetCore.Blazor/js/webAuthnComponentInterop.js").AsTask());
        }

        public async ValueTask CreateWebAuthn(Func<Task<StateFlag<BlazorClientWebAuthnOptions>>> callback, Func<BlazorCreateWebAuthnKeyResponse, Task<StateFlag>> createCallback)
        {
            var module = await moduleTask.Value;

            getCreateWebAuthnOptionsCallbackDotNetObjectRef?.Dispose();
            createWebAuthnCallbackDotNetObjectRef?.Dispose();

            getCreateWebAuthnOptionsCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsInvokeObjectHandlerDelegate<Task<StateFlag<BlazorClientWebAuthnOptions>>>(callback));
            createWebAuthnCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsInvokeCreateWebAuthnHandlerDelegate(createCallback));
            await module.InvokeVoidAsync("createWebAuthn", getCreateWebAuthnOptionsCallbackDotNetObjectRef, createWebAuthnCallbackDotNetObjectRef);
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
        }
    }
    public class JsInvokeCreateWebAuthnHandlerDelegate(Func<BlazorCreateWebAuthnKeyResponse, Task<StateFlag>> callback)
    {
        [JSInvokable]
        public Task<StateFlag> InvokeCallback(CreateWebAuthnKeyResponse data)
        {
            BlazorCreateWebAuthnKeyResponse response = Copy.New<BlazorCreateWebAuthnKeyResponse>(data);
            response.AttestationObjectData = WebAuthnParser.ParseAttestationObject(response.Response.AttestationObject);
            return callback != null ? callback.Invoke(response) : default;
        }
    }

}
