using Microsoft.AspNetCore.Builder;
using System;
using Silmoon.AspNetCore;
using Silmoon.AspNetCore.Middlewares;
using Silmoon.Extension.Interfaces;

namespace Silmoon.AspNetCore.Extensions
{
    [Obsolete]
    public static class IApplicationBuilderExtension
    {
        [Obsolete]
        public static IApplicationBuilder UseUserSession<TUser>(this IApplicationBuilder app, UserSessionManager.UserSessionHanlder<IDefaultUserIdentity> OnRecoveryUserData, UserSessionManager.UserTokenHanlder<IDefaultUserIdentity> OnRequestUserToken) where TUser : IDefaultUserIdentity
        {
            UserSessionManager.OnRequestUserData += OnRecoveryUserData;
            UserSessionManager.OnRequestUserToken += OnRequestUserToken;
            return app;
        }
        /// <summary>
        /// 使用基于 SilmoonDevApp 请求的API解密中间件，需要 ISilmoonDevAppService 服务
        /// </summary>
        /// <param name="app"></param>
        /// <param name="AppIdRenameTo"></param>
        /// <param name="KeyCacheSecound"></param>
        public static void UseApiDecrypt(this IApplicationBuilder app, string AppIdRenameTo = "AppId", int KeyCacheSecound = 3600)
        {
            app.UseMiddleware<ApiDecryptMiddleware>(AppIdRenameTo, KeyCacheSecound);
        }
        public static void UseSilmoonAuth(this IApplicationBuilder app)
        {
            app.UseMiddleware<SilmoonAuthMiddleware>();
        }
    }
}
