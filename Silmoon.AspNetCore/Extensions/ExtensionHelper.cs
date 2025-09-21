using Microsoft.AspNetCore.Mvc;
using Silmoon.Extension;
using Silmoon.Extension.Models;
using Silmoon.Models;
using System;

namespace Silmoon.AspNetCore.Extensions
{
    public static class ExtensionHelper
    {
        public static IActionResult GetStateResultJson(this bool success, string message = null)
        {
            StateResult stateResult = StateResult.Create(success, 0, message);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateResultJson(this bool success, int code, string message = null)
        {
            StateResult stateResult = StateResult.Create(success, code, message);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateResultJson<T>(this bool success, T data, string message = null)
        {
            StateResult<T> stateResult = StateResult<T>.Create(success, data, 0, message);
            return new ContentResult() { Content = stateResult.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateResultJson<T>(this bool success, T data, int code, string message = null)
        {
            StateResult<T> stateResult = StateResult<T>.Create(success, data, code, message);
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
