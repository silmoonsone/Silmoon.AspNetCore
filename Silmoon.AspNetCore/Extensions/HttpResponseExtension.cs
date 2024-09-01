using Microsoft.AspNetCore.Http;
using Silmoon.Extension;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Extensions
{
    public static class HttpResponseExtension
    {
        public static Task WriteJObjectAsync<T>(this HttpResponse httpResponse, T @object)
        {
            httpResponse.ContentType = "application/json";
            return httpResponse.WriteAsync(@object.ToJsonString());
        }
    }
}
