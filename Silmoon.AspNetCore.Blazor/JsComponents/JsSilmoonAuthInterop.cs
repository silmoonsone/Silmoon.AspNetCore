using Microsoft.JSInterop;
using Silmoon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Blazor.JsComponents
{
    public class JsSilmoonAuthInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public JsSilmoonAuthInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Silmoon.AspNetCore.Blazor/js/jsSilmoonAuthInterop.js").AsTask());
        }
        public async ValueTask<StateFlag<object>> SignIn(string username, string password)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<StateFlag<object>>("doSignIn", username, password);
        }
        public async ValueTask<StateFlag<object>> SignOut()
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<StateFlag<object>>("doSignOut");
        }
        public async ValueTask DisposeAsync()
        {
            // is a bug?
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                try
                {
                    await module.DisposeAsync();
                }
                catch { }
            }
        }
    }
}
