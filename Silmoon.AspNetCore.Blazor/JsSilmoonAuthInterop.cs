using Microsoft.JSInterop;
using Silmoon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Blazor
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
            return await module.InvokeAsync<StateFlag<object>>("doCreateSession", username, password);
        }
        public async ValueTask<StateFlag<object>> SignOut()
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<StateFlag<object>>("doClearSession");
        }
        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}
