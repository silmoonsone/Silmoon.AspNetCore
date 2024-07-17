using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Silmoon.Extension;
using System;
using System.Collections.Generic;
using System.IO;

namespace Silmoon.AspNetCore.Extensions
{
    public static class NavigationManagerExtension
    {
        public static Dictionary<string, StringValues> GetQueryStringValues(this NavigationManager navigationManager)
        {
            return QueryHelpers.ParseQuery(new Uri(navigationManager.Uri).Query);
        }
        public static StringValues GetQueryStringValue(this NavigationManager navigationManager, string key)
        {
            return QueryHelpers.ParseQuery(new Uri(navigationManager.Uri).Query).Get(key);
        }
        public static string GetQueryString(this NavigationManager navigationManager, string key)
        {
            var value = QueryHelpers.ParseQuery(new Uri(navigationManager.Uri).Query).Get(key);
            return value.Count == 0 ? null : value.ToString();
        }
    }
}
