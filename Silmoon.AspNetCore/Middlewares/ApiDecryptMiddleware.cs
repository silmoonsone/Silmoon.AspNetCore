using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Extensions;
using Silmoon.AspNetCore.Services;
using Silmoon.AspNetCore.Services.Interfaces;
using Silmoon.Extension;
using Silmoon.Extension.Models;
using Silmoon.Extension.Models.Types;
using Silmoon.Runtime.Cache;
using Silmoon.Secure;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Middlewares
{
    public class ApiDecryptMiddleware
    {
        /// <summary>
        /// 基于 SilmoonDevApp 请求的API解密中间件，需要 ISilmoonDevAppService 服务
        /// </summary>
        private RequestDelegate _next;
        public string AppId { get; set; }
        public int KeyCacheSecound { get; set; } = 3600;
        public ISilmoonDevAppService SilmoonDevAppService { get; set; }

        public ApiDecryptMiddleware(RequestDelegate next, ISilmoonDevAppService silmoonDevAppService, string appIdParameterName = "AppId", int keyCacheSecound = 3600)
        {
            _next = next;
            AppId = appIdParameterName;
            KeyCacheSecound = keyCacheSecound;
            SilmoonDevAppService = silmoonDevAppService;
        }
        public async Task Invoke(HttpContext context)
        {
            if ((context.Request.Headers["ApiEncrypt"].ToString().ToBool()
                || context.Request.Headers["SilmoonDevAppEncrypted"].ToString().ToBool())
                && context.Request.ContentType == "application/x-www-form-urlencoded")
            {
                try
                {
                    //读取正文
                    var str = await context.Request.GetBodyString();
                    //将正文序列化成JObject对象
                    var obj = JsonConvert.DeserializeObject<JObject>(str);

                    //从JObject对象中获取AppId、加密的正文、签名、验证值(验证值是正文和加密密钥计算的MD5)
                    var appId = obj[AppId].Value<string>();
                    var encryptedPayload = obj["Data"].Value<string>();
                    var signature = obj["Signature"].Value<string>();
                    var check = obj["Check"].Value<string>();

                    var encodeData = Convert.FromBase64String(encryptedPayload);




                    var silmoonAppDev = await SilmoonDevAppService.GetCachedKey(appId);
                    (string SignatureKey, string EncryptKey) = (silmoonAppDev.Data.SignatureKey, silmoonAppDev.Data.EncryptKey);

                    if (!silmoonAppDev.State)
                    {
                        await context.Response.WriteAsync(ApiResult.CreateToJsonString(ResultState.Fail, "Get Signature and EncryptKey fail(" + silmoonAppDev.Message + ")"));
                        return;
                    }
                    //计算Check（md5(payload + EncryptKey)）值
                    var vCheck = (encryptedPayload + EncryptKey).GetMD5Hash();
                    if (check != vCheck)
                    {
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(ApiResult.CreateToJsonString(ResultState.Fail, "Encrypt check fail"));
                        return;
                    }
                    //使用EncryptKey解密payload
                    var payload = EncryptHelper.AesDecryptBase64StringToString(encryptedPayload, EncryptKey);
                    //计算SignatureKey，即为解密的payload和签名密钥md5校验
                    var vSignature = (payload + SignatureKey).GetMD5Hash();
                    if (signature != vSignature)
                    {
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(ApiResult.CreateToJsonString(ResultState.Fail, "Encrypt signature error"));
                        return;
                    }

                    //验证请求内容(payload)的签名
                    var nameValues = UrlDataCollection.Parse(payload);
                    signature = nameValues["Signature"];
                    var csignature = nameValues.GetSign("Key", SignatureKey);
                    if (signature != csignature)
                    {
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(ApiResult.CreateToJsonString(ResultState.Fail, "Signature error"));
                        return;
                    }


                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
                    context.Request.Body = stream;

                    await _next(context);
                }
                catch (Exception ex)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(ApiResult.CreateToJsonString(ResultState.Exception, ex.ToString()));
                    return;
                }
            }
            else
                await _next(context);
        }

    }
}
