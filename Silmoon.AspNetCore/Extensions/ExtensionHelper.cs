using Microsoft.AspNetCore.Mvc;
using Silmoon.Extension;
using Silmoon.Extension.Models;
using Silmoon.Models;
using System;

namespace Silmoon.AspNetCore.Extensions
{
    public static class ExtensionHelper
    {
        public static IActionResult GetStateResultJson(this bool Success, string Message = null)
        {
            StateResult stateResult = StateResult.Create(Success, 0, Message);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateResultJson<T>(this bool Success, T Data = default, string Message = null)
        {
            StateResult<T> stateResult = StateResult<T>.Create(Success, Data, 0, Message);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateResultJson(this bool Success, int Code, string Message = null)
        {
            StateResult stateResult = StateResult.Create(Success, Code, Message);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateResultJson<T>(this bool Success, T Data = default, int Code = 0, string Message = null)
        {
            StateResult<T> stateResult = StateResult<T>.Create(Success, Data, Code, Message);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }

        public static IActionResult GetStateResultJson(this (bool Success, string Message) flag)
        {
            StateResult stateResult = StateResult.Create(flag.Success, 0, flag.Message);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateResultJson(this (bool Success, int Code) flag)
        {
            StateResult stateResult = StateResult.Create(flag.Success, flag.Code, default);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateResultJson<T>(this (bool Success, T Data) flag)
        {
            StateResult stateResult = StateResult<T>.Create(flag.Success, flag.Data, 0);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateResultJson<T>(this (bool Success, T Data, string Message) flag)
        {
            StateResult stateResult = StateResult<T>.Create(flag.Success, flag.Data, 0, flag.Message);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateResultJson<T>(this (bool Success, int Code, string Message) flag)
        {
            StateResult stateResult = StateResult<T>.Create(flag.Success, default, flag.Code, flag.Message);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateResultJson<T>(this (bool Success, T Data, int Code, string Message) flag)
        {
            StateResult stateResult = StateResult<T>.Create(flag.Success, flag.Data, flag.Code, flag.Message);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
    }
}
