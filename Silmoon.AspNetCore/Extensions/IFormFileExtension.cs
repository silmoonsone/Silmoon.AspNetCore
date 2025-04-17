using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Extensions
{
    public static class IFormFileExtension
    {
        public static async Task<byte[]> GetBytesAsync(this IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var result = new byte[file.Length];
            await stream.ReadExactlyAsync(result.AsMemory(0, (int)file.Length));
            return result;
        }
    }
}
