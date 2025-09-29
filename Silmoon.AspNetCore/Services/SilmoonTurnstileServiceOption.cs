using System.Collections.Generic;

namespace Silmoon.AspNetCore.Services
{
    public class SilmoonTurnstileServiceOption
    {
        public string SiteKey { get; set; }
        public string SecretKey { get; set; }
        public List<string> IgnorePathSuffixs { get; set; } = ["/_content/", "/lib/", "/js/", "/css/", "/images/", "/favicon.ico"];
        public List<string> IgnorePathKeywords { get; set; } = [];
        /// <summary>
        /// 用于加密 Cookie 的密钥，你需要重新设置一个新的以保证安全，必须是 32 字节长的字符串。
        /// </summary>
        public string CookieEncryptionKey { get; set; } = "PibTXpdfWdqiKkMOmrjLyujBAsnTdOBM";
    }
}
