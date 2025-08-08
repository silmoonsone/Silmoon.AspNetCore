using Microsoft.JSInterop;
using Silmoon.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Silmoon.AspNetCore.Blazor.JsComponents
{
    // This class provides an example of how JavaScript functionality can be wrapped
    // in a .NET class for easy consumption. The associated JavaScript module is
    // loaded on demand when first needed.
    //
    // This class can be registered as scoped DI service and then injected into Blazor
    // components for use.

    public class JsComponentInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        private DotNetObjectReference<JsInvokeStateSetHandlerDelegate> metroUIConfirmCallbackDotNetObjectRef;

        public JsComponentInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Silmoon.AspNetCore.Blazor/js/jsComponentInterop.js").AsTask());
        }

        public async ValueTask Alert(string message)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("alert", message);
        }
        public async ValueTask<bool> Confirm(string message)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<bool>("confirm", message);
        }
        public async ValueTask<string> Prompt(string message)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("showPrompt", message);
        }
        public async ValueTask<bool> CopyText(string text)
        {
            try
            {
                var module = await moduleTask.Value;
                return await module.InvokeAsync<bool>("copyText", text);
            }
            catch
            {
                return false;
            }
        }
        public async ValueTask<bool> CopyElementText(string elementId, bool clearSelected = true)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<bool>("copyElementText", elementId, clearSelected);
        }

        public async ValueTask Toast(string message, int delay = 1000)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("toast", message, delay);
        }

        public async ValueTask MetroUIConfirm(string title, string msg, bool isConfirmDialog = false, Action<bool> callback = null)
        {
            var module = await moduleTask.Value;
            if (callback is not null)
            {
                metroUIConfirmCallbackDotNetObjectRef?.Dispose();
                metroUIConfirmCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsInvokeStateSetHandlerDelegate(callback));
                await module.InvokeVoidAsync("metroUIConfirm", title, msg, isConfirmDialog, metroUIConfirmCallbackDotNetObjectRef);
            }
            else
            {
                await module.InvokeVoidAsync("metroUIConfirm", title, msg, isConfirmDialog);
                metroUIConfirmCallbackDotNetObjectRef?.Dispose();
                metroUIConfirmCallbackDotNetObjectRef = null;
            }
        }

        public async ValueTask<StateSet<bool>> Download(string fileName, byte[] content, string contentType, string contentTypeDescription = null)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<StateSet<bool>>("blazorDownloadFile", fileName, content, contentType, contentTypeDescription);
        }
        public async ValueTask<StateSet<bool>> Download(string fileName, string content, string contentType, string contentTypeDescription = null)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<StateSet<bool>>("blazorDownloadFile", fileName, content, contentType, contentTypeDescription);
        }
        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                try
                {
                    var module = await moduleTask.Value;
                    await module.DisposeAsync();
                }
                catch { }
            }
            try
            {
                metroUIConfirmCallbackDotNetObjectRef?.Dispose();
                metroUIConfirmCallbackDotNetObjectRef = null;
            }
            catch { }
        }
    }
}
