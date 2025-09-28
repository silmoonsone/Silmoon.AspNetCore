using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Silmoon.AspNetCore.Services;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Middlewares
{
    public class SilmoonTurnstileMiddleware(RequestDelegate next, SilmoonTurnstileService turnstileService, IOptions<SilmoonTurnstileServiceOption> option)
    {
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/SilmonTurnstile/challenge")
            {
                await turnstileService.Challenge(context, option.Value.SecretKey, context.Request.Form["token"]);
                return;
            }

            if (turnstileService.RequestChecking(context).State)
            {
                await next(context);
                return;
            }

            if (!turnstileService.Verify(context).State)
            {
                context.Request.Path = "/_content/Silmoon.AspNetCore/html/Turnstile/ChallengePage.html";

                // 替换响应流为内存流
                var originalBody = context.Response.Body;
                using var memStream = new MemoryStream();
                context.Response.Body = memStream;

                await next(context);

                // 读取内存流内容并替换
                memStream.Seek(0, SeekOrigin.Begin);
                var html = await new StreamReader(memStream).ReadToEndAsync();
                html = html.Replace("{{SilmoonTurnstileSiteKey}}", option.Value.SiteKey);

                // 写回原始流
                context.Response.Body = originalBody;
                context.Response.ContentLength = System.Text.Encoding.UTF8.GetByteCount(html);
                await context.Response.WriteAsync(html);
            }
            else
                await next(context);
        }
    }
}
