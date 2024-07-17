using Microsoft.AspNetCore.Http;
using Silmoon.Secure;
using System.Collections.Generic;

namespace Silmoon.AspNetCore.Extensions
{
    public static class IFormCollectionExtension
    {
        public static string GetSign(this IFormCollection keyValuePairs, string Key, string Value, string IgnoreKey = "signatrue")
        {
            List<string> array = new List<string>();

            foreach (var item in keyValuePairs)
            {
                if (item.Key.ToLower() == IgnoreKey.ToLower()) continue;
                array.Add(item.Key + "=" + item.Value);
            }

            array.Sort();
            string s = "";

            foreach (var item in array) s += item + "&";

            s += Key + "=" + Value;
            return HashHelper.GetMD5Hash(s);
        }
    }
}
