using Microsoft.AspNetCore.Mvc;
using Silmoon.Compress;
using Silmoon.Extension;
using Silmoon.Extension.Models;
using Silmoon.Extension.Models.Types;
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

        public static IActionResult JsonStateFlag(this ControllerBase controller, bool success) => JsonStateFlag(controller, success, 0, null);
        public static IActionResult JsonStateFlag(this ControllerBase controller, bool success, string message) => JsonStateFlag(controller, success, 0, message);
        public static IActionResult JsonStateFlag(this ControllerBase controller, bool success, int code) => JsonStateFlag(controller, success, code, null);
        public static IActionResult JsonStateFlag(this ControllerBase controller, bool success, int code, string message)
        {
            var result = StateFlag.Create(success, code, message);
            return new ContentResult() { Content = result.ToJsonString(), ContentType = "application/json" };
        }
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success) => JsonStateFlag<T>(controller, success, 0, string.Empty, default);
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, string message) => JsonStateFlag<T>(controller, success, 0, message, default);
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, int code) => JsonStateFlag<T>(controller, success, code, string.Empty, default);
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, int code, string message) => JsonStateFlag<T>(controller, success, code, message, default);
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, T data) => JsonStateFlag<T>(controller, success, 0, string.Empty, data);
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, string message, T data) => JsonStateFlag<T>(controller, success, 0, message, data);
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, int code, T data) => JsonStateFlag<T>(controller, success, code, string.Empty, data);
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, bool success, int code, string message, T data) => JsonStateFlag(controller, StateFlag<T>.Create(success, code, data, message));
        public static IActionResult JsonStateFlag<T>(this ControllerBase controller, StateFlag<T> stateFlag) => new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };




        public static IActionResult JsonApiResult(this ControllerBase controller, ResultState resultState, bool httpContentCompress = false) => JsonApiResult(controller, resultState, null, 0, httpContentCompress);
        public static IActionResult JsonApiResult(this ControllerBase controller, ResultState resultState, int code, bool httpContentCompress = false) => JsonApiResult(controller, resultState, null, code, httpContentCompress);
        public static IActionResult JsonApiResult(this ControllerBase controller, ResultState resultState, string message, bool httpContentCompress = false) => JsonApiResult(controller, resultState, message, 0, httpContentCompress);
        public static IActionResult JsonApiResult(this ControllerBase controller, ResultState resultState, string message = null, int code = 0, bool httpContentCompress = false) => JsonApiResult(controller, ApiResult.Create(resultState, message, code), httpContentCompress);
        public static IActionResult JsonApiResult(this ControllerBase controller, ApiResult apiResult, bool httpContentCompress = false)
        {
            if (httpContentCompress)
            {
                controller.Response.Headers["HttpContentGzipCompression"] = "true";
                var compressedData = CompressHelper.CompressStringToByteArray(apiResult.ToJsonString());
                return new ObjectResult(compressedData)
                {
                    ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection() { "application/json+gzip" },
                };
            }
            else
                return new ContentResult() { Content = apiResult.ToJsonString(), ContentType = "application/json" };
        }

        public static IActionResult JsonApiResult<T>(this ControllerBase controller, ResultState resultState, int code = 0, bool httpContentCompress = false) => JsonApiResult<T>(controller, resultState, default, null, code, httpContentCompress);
        public static IActionResult JsonApiResult<T>(this ControllerBase controller, ResultState resultState, string message, int code = 0, bool httpContentCompress = false) => JsonApiResult<T>(controller, resultState, default, message, code, httpContentCompress);
        public static IActionResult JsonApiResult<T>(this ControllerBase controller, ResultState resultState, T data, int code = 0, bool httpContentCompress = false) => JsonApiResult<T>(controller, resultState, data, null, code, httpContentCompress);
        public static IActionResult JsonApiResult<T>(this ControllerBase controller, ResultState resultState, T data, string Message, int code = 0, bool httpContentCompress = false) => JsonApiResult(controller, ApiResult<T>.Create(resultState, data, Message, code), httpContentCompress);
        public static IActionResult JsonApiResult<T>(this ControllerBase controller, ApiResult<T> apiResult, bool httpContentCompress = false)
        {
            if (httpContentCompress)
            {
                controller.Response.Headers["HttpContentGzipCompression"] = "true";
                var compressedData = CompressHelper.CompressStringToByteArray(apiResult.ToJsonString());
                return new ObjectResult(compressedData)
                {
                    ContentTypes = new Microsoft.AspNetCore.Mvc.Formatters.MediaTypeCollection() { "application/json+gzip" },
                };
            }
            else
                return new ContentResult() { Content = apiResult.ToJsonString(), ContentType = "application/json" };

        }
    }
}
