using System.Collections.Generic;

namespace Silmoon.AspNetCore.Services
{
    public class SilmoonTurnstileServiceOption
    {
        public string SiteKey { get; set; }
        public string SecretKey { get; set; }
        public List<string> IgnorePathSuffixs { get; set; } = ["/_content/", "/js/", "/css/", "/images/", "/favicon.ico"];
        public List<string> IgnorePathKeywords { get; set; } = [];
    }
}
