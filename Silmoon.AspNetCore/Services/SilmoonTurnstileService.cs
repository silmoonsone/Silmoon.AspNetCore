using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Silmoon.AspNetCore.Interfaces;
using Silmoon.Collections;
using Silmoon.Extension;
using Silmoon.Extension.Http;
using Silmoon.Models;
using Silmoon.Secure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Services
{
    public class SilmoonTurnstileService
    {
        Dictionary<string, List<string>> HashedCookies = [];
        SilmoonTurnstileServiceOption Option { get; set; }
        public SilmoonTurnstileService(IOptions<SilmoonTurnstileServiceOption> option)
        {
            Option = option.Value;
        }
        public StateSet<bool> RequestChecking(HttpContext httpContext)
        {
            var isIgnorePath = Option.IgnorePathSuffixs.Any(httpContext.Request.Path.Value.Contains);
            if (isIgnorePath) return true.ToStateSet("The path suffix is ignore.");
            var isIgnoreKeyword = Option.IgnorePathKeywords.Any(httpContext.Request.Path.Value.Contains);
            if (isIgnoreKeyword) return true.ToStateSet("The path keyword is ignore.");
            return false.ToStateSet("Can not ignore.");
        }
        public StateSet<bool> Verify(HttpContext httpContext)
        {
            var cookie = httpContext.Request.Cookies["____SilmoonTurnstile"];
            if (cookie.IsNullOrEmpty()) return false.ToStateSet("No cookie.");
            var str = EncryptHelper.AesDecryptStringV2(cookie, Option.CookieEncryptionKey);
            if (str.IsNullOrEmpty()) return false.ToStateSet("Cookie is invalid.");
            var strArray = str?.Split('|');
            if (strArray.Length != 3) return false.ToStateSet("Cookie is invalid.");

            var strip = strArray[0];
            var strUserAgent = strArray[1];
            var strRandom = strArray[2];

            var ip = httpContext.Connection.RemoteIpAddress?.ToString();
            if (strip != ip) return false.ToStateSet("Cookie is invalid.");
            if (strUserAgent != httpContext.Request.Headers.UserAgent.ToString()) return false.ToStateSet("Cookie is valid.");
            return true.ToStateSet();
        }
        public async Task<StateSet<bool, TurnstileResult>> Challenge(HttpContext httpContext, string secret, string token)
        {
            var verifyResult = await TurnstileApi.Verify(secret, token);
            var result = (verifyResult.IsSuccess && verifyResult.Result.Success).ToStateSet(verifyResult.Result, verifyResult.Exception?.ToString() ?? "");

            var ip = httpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            var str = $"{ip}|{userAgent}|{HashHelper.RandomChars(16)}";
            var cookie = EncryptHelper.AesEncryptStringV2(str, Option.CookieEncryptionKey);

            if (result.State) httpContext.Response.Cookies.Append("____SilmoonTurnstile", cookie);
            await httpContext.Response.WriteAsJsonAsync(result.State.ToStateResult(result.Data, result.Message));

            //var ipCookies = HashedCookies.Get(ip);
            //if (ipCookies is null) HashedCookies[ip] = [];
            //else
            //{
            //    if (ipCookies.Count >= 100) ipCookies.Clear();
            //}
            //HashedCookies[ip].Add(cookie);
            return result;
        }
    }
    public class TurnstileApi
    {
        public static async Task<JsonRequestResult<TurnstileResult>> Verify(string secret, string response)
        {
            var url = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
            var data = new UrlDataCollection
            {
                { "secret", secret },
                { "response", response }
            };

            var result = await JsonRequest.PostFormDataAsync<TurnstileResult>(url, data, null);
            return result;
        }
    }
    public class TurnstileResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("error-codes")]
        public string[] ErrorCodes { get; set; }
        [JsonProperty("challenge_ts")]
        public DateTime ChallengeTs { get; set; }
        [JsonProperty("hostname")]
        public string Hostname { get; set; }
    }

}
