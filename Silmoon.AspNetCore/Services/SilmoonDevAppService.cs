using Microsoft.Extensions.Options;
using Silmoon.AspNetCore.Services.Interfaces;
using Silmoon.Models;
using Silmoon.Runtime.Cache;
using System;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Services
{
    public abstract class SilmoonDevAppService : ISilmoonDevAppService
    {
        private SilmoonDevAppServiceOptions Options { get; set; }

        public SilmoonDevAppService(IOptions<SilmoonDevAppServiceOptions> options)
        {
            Options = options.Value is null ? new SilmoonDevAppServiceOptions() : options.Value;
        }

        public async Task<StateSet<bool, (string SignatureKey, string EncryptKey)>> GetCachedKey(string AppId)
        {
            var cacheKeyResult = GlobalCaching<string, (string SignatureKey, string EncryptKey)>.Get("__" + AppId + "__S.E.Key_cache");

            //在缓存中找到Signature和EncryptKey
            if (cacheKeyResult.Matched)
            {
                GlobalCaching<string, (string, string)>.Set("__" + AppId + "__S.E.Key_cache", (cacheKeyResult.Value.SignatureKey, cacheKeyResult.Value.EncryptKey), DateTime.Now.AddSeconds(Options.KeyCacheSecoundTimeout));
                return StateSet<bool, (string SignatureKey, string EncryptKey)>.Create(true, (cacheKeyResult.Value.SignatureKey, cacheKeyResult.Value.EncryptKey), "cached.");
            }
            else
            {
                //没有在缓存中找到Signature和EncryptKey
                var result = await GetKey(AppId);
                if (result.State)
                {
                    if (Options.KeyCacheSecoundTimeout > 0) GlobalCaching<string, (string, string)>.Set("__" + AppId + "__S.E.Key_cache", (result.Data.SignatureKey, result.Data.EncryptKey), DateTime.Now.AddSeconds(Options.KeyCacheSecoundTimeout));
                    return StateSet<bool, (string SignatureKey, string EncryptKey)>.Create(true, (result.Data.SignatureKey, result.Data.EncryptKey));
                }
                else
                {
                    //没有获取到Signature和EncryptKey
                    return StateSet<bool, (string SignatureKey, string EncryptKey)>.Create(false, (null, null), "Get SignatureKey and EncryptKey failed(" + result.Message + ").");
                }
            }

        }
        public abstract Task<StateSet<bool, (string SignatureKey, string EncryptKey)>> GetKey(string AppId);
    }
}
