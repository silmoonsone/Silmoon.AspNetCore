using Microsoft.AspNetCore.Mvc;
using Silmoon.Compress;
using Silmoon.Extension;
using Silmoon.Extension.Models;
using Silmoon.Extension.Enums;
using Silmoon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Extensions
{
    public static class ControllerBaseExtension
    {
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag(this ControllerBase controller, bool success) => JsonStateFlag(controller, success, 0, null);
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag(this ControllerBase controller, bool success, string message) => JsonStateFlag(controller, success, 0, message);
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag(this ControllerBase controller, bool success, int code) => JsonStateFlag(controller, success, code, null);
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag(this ControllerBase controller, bool success, int code, string message) => JsonStateFlag(controller, StateFlag.Create(success, code, message));
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag(this ControllerBase controller, StateFlag stateFlag) => controller.Content(stateFlag.ToJsonString(), "application/json");

        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success) => JsonStateFlag<T>(controller, success, 0, string.Empty, default);
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, string message) => JsonStateFlag<T>(controller, success, 0, message, default);
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, int code) => JsonStateFlag<T>(controller, success, code, string.Empty, default);
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, int code, string message) => JsonStateFlag<T>(controller, success, code, message, default);
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, T data) => JsonStateFlag(controller, success, 0, string.Empty, data);
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, string message, T data) => JsonStateFlag(controller, success, 0, message, data);
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, int code, T data) => JsonStateFlag(controller, success, code, string.Empty, data);
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, int code, string message, T data) => JsonStateFlag(controller, StateFlag<T>.Create(success, code, data, message));
        [Obsolete("Use JsonStateFlag or JsonStateResult instead.")]
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, StateFlag<T> stateFlag) => controller.Content(stateFlag.ToJsonString(), "application/json");



        public static IActionResult JsonStateResult(this ControllerBase controller, bool success) => JsonStateResult(controller, success, 0, null);
        public static IActionResult JsonStateResult(this ControllerBase controller, bool success, string message) => JsonStateResult(controller, success, 0, message);
        public static IActionResult JsonStateResult(this ControllerBase controller, bool success, int code) => JsonStateResult(controller, success, code, null);
        public static IActionResult JsonStateResult(this ControllerBase controller, bool success, int code, string message) => JsonStateResult(controller, StateResult.Create(success, code, message));
        public static IActionResult JsonStateResult(this ControllerBase controller, StateResult stateResult) => controller.Content(stateResult.ToJsonString(), "application/json");

        public static IActionResult JsonStateResult<T>(this ControllerBase controller, bool success) => JsonStateResult<T>(controller, success, 0, string.Empty, default);
        public static IActionResult JsonStateResult<T>(this ControllerBase controller, bool success, string message) => JsonStateResult<T>(controller, success, 0, message, default);
        public static IActionResult JsonStateResult<T>(this ControllerBase controller, bool success, int code) => JsonStateResult<T>(controller, success, code, string.Empty, default);
        public static IActionResult JsonStateResult<T>(this ControllerBase controller, bool success, int code, string message) => JsonStateResult<T>(controller, success, code, message, default);
        public static IActionResult JsonStateResult<T>(this ControllerBase controller, bool success, T data) => JsonStateResult(controller, success, 0, string.Empty, data);
        public static IActionResult JsonStateResult<T>(this ControllerBase controller, bool success, string message, T data) => JsonStateResult(controller, success, 0, message, data);
        public static IActionResult JsonStateResult<T>(this ControllerBase controller, bool success, int code, T data) => JsonStateResult(controller, success, code, string.Empty, data);
        public static IActionResult JsonStateResult<T>(this ControllerBase controller, bool success, int code, string message, T data) => JsonStateResult(controller, StateResult<T>.Create(success, data, code, message));
        public static IActionResult JsonStateResult<T>(this ControllerBase controller, StateResult<T> stateResult) => controller.Content(stateResult.ToJsonString(), "application/json");


        public static IActionResult JsonApiResult(this ControllerBase controller, ResultState resultState, bool httpContentCompress = false) => JsonApiResult(controller, resultState, 0, null, httpContentCompress);
        public static IActionResult JsonApiResult(this ControllerBase controller, ResultState resultState, int code, bool httpContentCompress = false) => JsonApiResult(controller, resultState, code, null, httpContentCompress);
        public static IActionResult JsonApiResult(this ControllerBase controller, ResultState resultState, string message, bool httpContentCompress = false) => JsonApiResult(controller, resultState, 0, message: message, httpContentCompress);
        public static IActionResult JsonApiResult(this ControllerBase controller, ResultState resultState, int code, string message, bool httpContentCompress = false) => JsonApiResult(controller, ApiResult.Create(resultState, message, code), httpContentCompress);
        public static IActionResult JsonApiResult(this ControllerBase controller, ApiResult apiResult, bool httpContentCompress = false)
        {
            if (httpContentCompress)
            {
                controller.Response.Headers["HttpContentGzipCompression"] = "true";
                var compressedData = CompressHelper.CompressStringToByteArray(apiResult.ToJsonString());
                return new FileContentResult(compressedData, "application/json+gzip");
            }
            else
                return new ContentResult() { Content = apiResult.ToJsonString(), ContentType = "application/json" };
        }


        public static IActionResult JsonApiResult<T>(this ControllerBase controller, ResultState resultState, T data, bool httpContentCompress = false) => JsonApiResult(controller, resultState, data, 0, string.Empty, httpContentCompress);
        public static IActionResult JsonApiResult<T>(this ControllerBase controller, ResultState resultState, T data, int code, bool httpContentCompress = false) => JsonApiResult(controller, resultState, data, code, string.Empty, httpContentCompress);
        public static IActionResult JsonApiResult<T>(this ControllerBase controller, ResultState resultState, T data, string message, bool httpContentCompress = false) => JsonApiResult(controller, resultState, data, 0, message, httpContentCompress);
        public static IActionResult JsonApiResult<T>(this ControllerBase controller, ResultState resultState, T data, int code, string message, bool httpContentCompress = false) => JsonApiResult(controller, ApiResult<T>.Create(resultState, data, message, code), httpContentCompress);
        public static IActionResult JsonApiResult<T>(this ControllerBase controller, ApiResult<T> apiResult, bool httpContentCompress = false)
        {
            if (httpContentCompress)
            {
                controller.Response.Headers["HttpContentGzipCompression"] = "true";
                var compressedData = CompressHelper.CompressStringToByteArray(apiResult.ToJsonString());
                return controller.File(compressedData, "application/json+gzip");
            }
            else
                return controller.Content(apiResult.ToJsonString(), "application/json");
        }
    }
}
