using System;

namespace Silmoon.AspNetCore.Services
{
    public class SilmoonAuthServiceOption
    {
        public string SignInUrl { get; set; } = "/_session/signIn";
        public string SignOutUrl { get; set; } = "/_session/signOut";
    }
}
