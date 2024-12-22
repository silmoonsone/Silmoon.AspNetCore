using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Enums;
using Silmoon.Extension;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Silmoon.AspNetCore.Extensions
{
    public static class HttpRequestExtension
    {
        public static string Url { get; set; }
        public static string GetRawUrl(this HttpRequest httpRequest) => httpRequest.GetEncodedUrl();
        public static bool IsAjaxRequest(this HttpRequest httpRequest) => string.Equals(httpRequest.Query[HeaderNames.XRequestedWith], "XMLHttpRequest", StringComparison.Ordinal) || string.Equals(httpRequest.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.Ordinal);

        public static string UserAgent(this HttpRequest httpRequest) => httpRequest.Headers["User-Agent"];
        public static bool IsIOS(this HttpRequest httpRequest) => httpRequest.UserAgent().Contains("iPhone") || httpRequest.UserAgent().Contains("iPad");
        public static bool IsAndroid(this HttpRequest httpRequest) => httpRequest.UserAgent().Contains("Android");
        public static bool IsWeixinBrowser(this HttpRequest httpRequest) => httpRequest.Headers["User-Agent"].ToString().Contains("MicroMessenger");
        public static ClientBrowserType GetMobileBrowserPlatform(this HttpRequest httpRequest)
        {
            var userAgent = httpRequest.Headers["User-Agent"].ToString();

            var androidRegex = new Regex(@"android", RegexOptions.IgnoreCase);
            var iosRegex = new Regex(@"(iPad|iPod|iPhone)", RegexOptions.IgnoreCase);

            if (androidRegex.IsMatch(userAgent))
                return ClientBrowserType.Android;
            else if (iosRegex.IsMatch(userAgent))
                return ClientBrowserType.IOS;

            return ClientBrowserType.Other;
        }
        public static JObject ReadToJson(this HttpRequest httpRequest) => JsonConvert.DeserializeObject<JObject>(httpRequest.Body.MakeToString());
        public static XmlDocument ReadToXml(this HttpRequest httpRequest)
        {
            var requestBody = httpRequest.Body.MakeToString();
            XmlDocument xml = new XmlDocument();
            xml.LoadXmlWithoutException(requestBody);
            return xml;
        }
        public static bool IsMobileDevice(this HttpRequest httpRequest)
        {
            string[] mobileAgents = { "iphone", "android", "phone", "mobile", "wap", "netfront", "java", "opera mobi", "opera mini", "ucweb", "windows ce", "symbian", "series", "webos", "sony", "blackberry", "dopod", "nokia", "samsung", "palmsource", "xda", "pieplus", "meizu", "midp", "cldc", "motorola", "foma", "docomo", "up.browser", "up.link", "blazer", "helio", "hosin", "huawei", "novarra", "coolpad", "webos", "techfaith", "palmsource", "alcatel", "amoi", "ktouch", "nexian", "ericsson", "philips", "sagem", "wellcom", "bunjalloo", "maui", "smartphone", "iemobile", "spice", "bird", "zte-", "longcos", "pantech", "gionee", "portalmmm", "jig browser", "hiptop", "benq", "haier", "^lct", "320x320", "240x320", "176x220", "w3c ", "acs-", "alav", "alca", "amoi", "audi", "avan", "benq", "bird", "blac", "blaz", "brew", "cell", "cldc", "cmd-", "dang", "doco", "eric", "hipt", "inno", "ipaq", "java", "jigs", "kddi", "keji", "leno", "lg-c", "lg-d", "lg-g", "lge-", "maui", "maxo", "midp", "mits", "mmef", "mobi", "mot-", "moto", "mwbp", "nec-", "newt", "noki", "oper", "palm", "pana", "pant", "phil", "play", "port", "prox", "qwap", "sage", "sams", "sany", "sch-", "sec-", "send", "seri", "sgh-", "shar", "sie-", "siem", "smal", "smar", "sony", "sph-", "symb", "t-mo", "teli", "tim-", "tosh", "tsm-", "upg1", "upsi", "vk-v", "voda", "wap-", "wapa", "wapi", "wapp", "wapr", "webc", "winw", "winw", "xda", "xda-", "googlebot-mobile" };
            bool isMoblie = false;
            string userAgent = httpRequest.UserAgent().ToLower();

            //排除 Windows 桌面系统或苹果桌面系统 
            if (!string.IsNullOrEmpty(userAgent) && !userAgent.Contains("macintosh") && (!userAgent.Contains("windows nt") || (userAgent.Contains("windows nt") && userAgent.Contains("compatible; msie 9.0;"))))
            {
                for (int i = 0; i < mobileAgents.Length; i++)
                {
                    if (userAgent.ToLower().IndexOf(mobileAgents[i]) >= 0)
                    {
                        isMoblie = true;
                        break;
                    }
                }
            }
            return isMoblie;
        }
        public static NameValueCollection GetQueryStringNameValues(this HttpRequest httpRequest)
        {
            var queries = httpRequest.Query;
            NameValueCollection nameValueCollection = new NameValueCollection();
            foreach (var item in queries)
            {
                nameValueCollection.Add(item.Key, item.Value);
            }
            return nameValueCollection;
        }
        public static NameValueCollection GetFormNameValues(this HttpRequest httpRequest)
        {
            var forms = httpRequest.Form;
            NameValueCollection nameValueCollection = new NameValueCollection();
            foreach (var item in forms)
            {
                nameValueCollection.Add(item.Key, item.Value);
            }
            return nameValueCollection;
        }
        [Obsolete("This method is obsolete, please use GetRequestValue method instead.")]
        public static string GetQueryStringOrFormValue(this HttpRequest httpRequest, string key)
        {
            var value = httpRequest.Query[key];
            if (value.Count == 0 && (httpRequest.ContentType.StartsWith("multipart/form-data") || httpRequest.ContentType == "application/x-www-form-urlencoded"))
                value = httpRequest.Form[key];
            return value;
        }
        public static string GetRequestValue(this HttpRequest httpRequest, string key)
        {
            var value = httpRequest.Query[key];
            if (value.Count == 0 && (httpRequest.ContentType.StartsWith("multipart/form-data") || httpRequest.ContentType == "application/x-www-form-urlencoded"))
                value = httpRequest.Form[key];
            return value;
        }
        public static async Task<string> GetBodyString(this HttpRequest httpRequest, Encoding encoding = null)
        {
            if (!httpRequest.Body.CanSeek)
            {
                // We only do this if the stream isn't *already* seekable,
                // as EnableBuffering will create a new stream instance
                // each time it's called
                httpRequest.EnableBuffering();
            }

            httpRequest.Body.Position = 0;
            using (var reader = new StreamReader(httpRequest.Body, encoding ?? Encoding.UTF8))
            {
                var body = await reader.ReadToEndAsync().ConfigureAwait(false);
                httpRequest.Body.Position = 0;
                return body;
            }
        }
        public static async Task<byte[]> GetBodyBytes(this HttpRequest httpRequest)
        {
            if (!httpRequest.Body.CanSeek)
            {
                // We only do this if the stream isn't *already* seekable,
                // as EnableBuffering will create a new stream instance
                // each time it's called
                httpRequest.EnableBuffering();
            }

            httpRequest.Body.Position = 0;
            return await httpRequest.Body.ToBytesAsync(httpRequest.ContentLength).ConfigureAwait(false);
        }
    }
}
