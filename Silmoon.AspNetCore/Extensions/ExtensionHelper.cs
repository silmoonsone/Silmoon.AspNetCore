using Microsoft.AspNetCore.Mvc;
using Silmoon.Extension;
using Silmoon.Models;

namespace Silmoon.AspNetCore.Extensions
{
    public static class ExtensionHelper
    {
        public static IActionResult GetStateFlagResult(this bool Success, string Message = null)
        {
            StateFlag stateFlag = StateFlag.Create(Success, 0, Message);
            return new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateFlagResult<T>(this bool Success, T Data = default, string Message = null)
        {
            StateFlag<T> stateFlag = StateFlag<T>.Create(Success, 0, Data, Message);
            return new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateFlagResult(this bool Success, int Code, string Message = null)
        {
            StateFlag stateFlag = StateFlag.Create(Success, Code, Message);
            return new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateFlagResult<T>(this bool Success, int Code = 0, T Data = default, string Message = null)
        {
            StateFlag<T> stateFlag = StateFlag<T>.Create(Success, Code, Data, Message);
            return new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };
        }

        public static IActionResult GetStateFlagResult(this (bool Success, string Message) flag)
        {
            StateFlag stateFlag = StateFlag.Create(flag.Success, 0, flag.Message);
            return new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateFlagResult(this (bool Success, int Code) flag)
        {
            StateFlag stateFlag = StateFlag.Create(flag.Success, flag.Code, default);
            return new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateFlagResult<T>(this (bool Success, T Data) flag)
        {
            StateFlag stateFlag = StateFlag<T>.Create(flag.Success, 0, flag.Data);
            return new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateFlagResult<T>(this (bool Success, T Data, string Message) flag)
        {
            StateFlag stateFlag = StateFlag<T>.Create(flag.Success, 0, flag.Data, flag.Message);
            return new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateFlagResult<T>(this (bool Success, int Code, string Message) flag)
        {
            StateFlag stateFlag = StateFlag<T>.Create(flag.Success, flag.Code, default, flag.Message);
            return new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult GetStateFlagResult<T>(this (bool Success, int Code, T Data, string Message) flag)
        {
            StateFlag stateFlag = StateFlag<T>.Create(flag.Success, flag.Code, flag.Data, flag.Message);
            return new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };
        }
    }
}
