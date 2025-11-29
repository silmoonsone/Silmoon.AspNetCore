using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Silmoon.AspNetCore.Services;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Silmoon.AspNetCore.Middlewares
{
    public class SilmoonGlobalTurnstileMiddleware(RequestDelegate next, SilmoonGlobalTurnstileService turnstileService, IOptions<SilmoonGlobalTurnstileServiceOption> option, IWebHostEnvironment env)
    {
        // 缓存挑战页模板（包含占位符），首次读取后缓存
        private string _challengeTemplateHtml;
        private readonly SemaphoreSlim _templateLoadLock = new(1, 1);

        private async Task<string> GetChallengeTemplateHtmlAsync()
        {
            if (_challengeTemplateHtml is not null)
                return _challengeTemplateHtml;

            await _templateLoadLock.WaitAsync();
            try
            {
                if (_challengeTemplateHtml is not null)
                    return _challengeTemplateHtml;

                // 读取静态资源（或嵌入资源）作为模板
                var filePath = "/_content/Silmoon.AspNetCore/html/Turnstile/ChallengePage.html";
                var fileInfo = env.WebRootFileProvider.GetFileInfo(filePath);

                string html;
                if (fileInfo.Exists)
                {
                    using var stream = fileInfo.CreateReadStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                    html = await reader.ReadToEndAsync();
                }
                else
                {
                    var asm = Assembly.GetExecutingAssembly();
                    var resourceName = asm.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("ChallengePage.html"));
                    if (resourceName is not null)
                    {
                        using var stream = asm.GetManifestResourceStream(resourceName)!;
                        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                        html = await reader.ReadToEndAsync();
                    }
                    else
                    {
                        html = null;
                    }
                }

                _challengeTemplateHtml = html;
                return _challengeTemplateHtml;
            }
            finally
            {
                _templateLoadLock.Release();
            }
        }

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
                // 使用缓存的模板，按需替换站点密钥
                var template = await GetChallengeTemplateHtmlAsync();
                if (template is null)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Challenge page not found.");
                    return;
                }

                var html = template.Replace("{{SilmoonTurnstileSiteKey}}", option.Value.SiteKey);

                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength = Encoding.UTF8.GetByteCount(html);
                await context.Response.WriteAsync(html, Encoding.UTF8);
                return;
            }
            else
                await next(context);
        }
    }
}
