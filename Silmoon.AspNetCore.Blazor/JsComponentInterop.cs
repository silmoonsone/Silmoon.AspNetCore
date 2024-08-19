using Microsoft.JSInterop;

namespace Silmoon.AspNetCore.Blazor
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
        private DotNetObjectReference<JsCallbackHelper> metroUIConfirmCallbackDotNetObjectRef;

        public JsComponentInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Silmoon.AspNetCore.Blazor/js/jsComponentInterop.js").AsTask());
        }

        public async ValueTask<string> Prompt(string message)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("showPrompt", message);
        }
        public async ValueTask Toast(string message, int delay = 1000)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("toast", message, delay);
        }
        public async ValueTask MetroUIConfirm(string title, string msg, bool isConfirm = false, Action<bool> callback = null)
        {
            var module = await moduleTask.Value;
            if (callback is not null)
            {
                if (metroUIConfirmCallbackDotNetObjectRef is null) metroUIConfirmCallbackDotNetObjectRef = DotNetObjectReference.Create(new JsCallbackHelper(callback));
                await module.InvokeVoidAsync("metroUIConfirm", title, msg, isConfirm, metroUIConfirmCallbackDotNetObjectRef);
            }
            else
            {
                await module.InvokeVoidAsync("metroUIConfirm", title, msg, isConfirm);
            }
        }
        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
            if (metroUIConfirmCallbackDotNetObjectRef is not null)
            {
                metroUIConfirmCallbackDotNetObjectRef?.Dispose();
                metroUIConfirmCallbackDotNetObjectRef = null;
            }
        }
    }

    public class JsCallbackHelper(Action<bool> callback)
    {
        private readonly Action<bool> _callback = callback;
        [JSInvokable]
        public void InvokeCallback(bool result) => _callback?.Invoke(result);
    }
}
