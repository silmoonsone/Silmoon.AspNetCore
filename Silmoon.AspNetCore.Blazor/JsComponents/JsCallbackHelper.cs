using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Blazor.JsComponents
{
    public class JsInvokeStateSetHandlerDelegate(Action<bool> callback)
    {
        [JSInvokable]
        public void InvokeCallback(bool result) => callback?.Invoke(result);
    }
    public class JsInvokeStateSetHandlerDelegate<TData>(Action<bool, TData, string> callback)
    {
        [JSInvokable]
        public void InvokeCallback(bool success, TData data, string message) => callback?.Invoke(success, data, message);
    }
    public class JsVoidInvokeObjectHandlerDelegate<T>(Action<T> callback)
    {
        [JSInvokable]
        public void InvokeCallback(T data) => callback?.Invoke(data);
    }
    public class JsInvokeObjectHandlerDelegate<TReturn>(Func<TReturn> callback)
    {
        [JSInvokable]
        public TReturn InvokeCallback() => callback != null ? callback.Invoke() : default;
    }
    public class JsInvokeObjectHandlerDelegate<T, TReturn>(Func<T, TReturn> callback)
    {
        [JSInvokable]
        public TReturn InvokeCallback(T data) => callback != null ? callback.Invoke(data) : default;
    }
}
