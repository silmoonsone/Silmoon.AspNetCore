using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;

namespace Silmoon.AspNetCore.Extensions
{
    public static class SessionExtension
    {
        public static bool GetBool(this ISession session, string key)
        {
            return session.GetInt32(key) == 1 ? true : false;
        }
        public static void SetBool(this ISession session, string key, bool value)
        {
            if (value) session.SetInt32(key, 1); else session.SetInt32(key, 0);
        }
    }
}
