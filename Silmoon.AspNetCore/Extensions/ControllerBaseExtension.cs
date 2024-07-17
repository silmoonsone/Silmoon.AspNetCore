using Microsoft.AspNetCore.Mvc;
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

        public static ContentResult JsonStateFlag(this ControllerBase controller, bool Success) => JsonStateFlag(controller, Success, 0, null);
        public static ContentResult JsonStateFlag(this ControllerBase controller, bool Success, string Message) => JsonStateFlag(controller, Success, 0, Message);
        public static ContentResult JsonStateFlag(this ControllerBase controller, bool Success, int Code) => JsonStateFlag(controller, Success, Code, null);
        public static ContentResult JsonStateFlag(this ControllerBase controller, bool Success, int Code, string Message)
        {
            var result = StateFlag.Create(Success, Code, Message);
            return new ContentResult() { Content = result.ToJsonString(), ContentType = "application/json" };
        }
        public static ContentResult JsonStateFlag<T>(this ControllerBase controller, bool Success) => JsonStateFlag<T>(controller, Success, 0, "", default);
        public static ContentResult JsonStateFlag<T>(this ControllerBase controller, bool Success, string Message) => JsonStateFlag<T>(controller, Success, 0, Message, default);
        public static ContentResult JsonStateFlag<T>(this ControllerBase controller, bool Success, int Code) => JsonStateFlag<T>(controller, Success, Code, "", default);
        public static ContentResult JsonStateFlag<T>(this ControllerBase controller, bool Success, int Code, string Message) => JsonStateFlag<T>(controller, Success, Code, Message, default);
        public static ContentResult JsonStateFlag<T>(this ControllerBase controller, bool Success, T Data) => JsonStateFlag<T>(controller, Success, 0, "", Data);
        public static ContentResult JsonStateFlag<T>(this ControllerBase controller, bool Success, string Message, T Data) => JsonStateFlag<T>(controller, Success, 0, Message, Data);
        public static ContentResult JsonStateFlag<T>(this ControllerBase controller, bool Success, int Code, T Data) => JsonStateFlag<T>(controller, Success, Code, "", Data);
        public static ContentResult JsonStateFlag<T>(this ControllerBase controller, bool Success, int Code, string Message, T Data) => JsonStateFlag(controller, StateFlag<T>.Create(Success, Code, Data, Message));
        public static ContentResult JsonStateFlag<T>(this ControllerBase controller, StateFlag<T> stateFlag) => new ContentResult() { Content = stateFlag.ToJsonString(), ContentType = "application/json" };




        public static ContentResult JsonApiResult(this ControllerBase controller, ResultState ResultState) => JsonApiResult(controller, ResultState, null, 0);
        public static ContentResult JsonApiResult(this ControllerBase controller, ResultState ResultState, int Code) => JsonApiResult(controller, ResultState, null, Code);
        public static ContentResult JsonApiResult(this ControllerBase controller, ResultState ResultState, string Message) => JsonApiResult(controller, ResultState, Message, 0);
        public static ContentResult JsonApiResult(this ControllerBase controller, ResultState ResultState, string Message = null, int Code = 0) => JsonApiResult(controller, ApiResult.Create(ResultState, Message, Code));
        public static ContentResult JsonApiResult(this ControllerBase controller, ApiResult apiResult) => new ContentResult() { Content = apiResult.ToJsonString(), ContentType = "application/json" };


        public static ContentResult JsonApiResult<T>(this ControllerBase controller, ResultState ResultState, int Code = 0) => JsonApiResult<T>(controller, ResultState, default, null, Code);
        public static ContentResult JsonApiResult<T>(this ControllerBase controller, ResultState ResultState, string Message, int Code = 0) => JsonApiResult<T>(controller, ResultState, default, Message, Code);
        public static ContentResult JsonApiResult<T>(this ControllerBase controller, ResultState ResultState, T Data, int Code = 0) => JsonApiResult<T>(controller, ResultState, Data, null, Code);
        public static ContentResult JsonApiResult<T>(this ControllerBase controller, ResultState ResultState, T Data, string Message, int Code = 0) => JsonApiResult(controller, ApiResult<T>.Create(ResultState, Data, Message, Code));
        public static ContentResult JsonApiResult<T>(this ControllerBase controller, ApiResult<T> apiResult) => new ContentResult() { Content = apiResult.ToJsonString(), ContentType = "application/json" };
    }
}
