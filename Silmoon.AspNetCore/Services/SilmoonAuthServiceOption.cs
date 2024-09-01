using System;

namespace Silmoon.AspNetCore.Services
{
    public class SilmoonAuthServiceOption
    {
        public string CreateSessionUrl { get; set; } = "/_session/createSession";
        public string ClearSessionUrl { get; set; } = "/_session/clearSession";
    }
}
