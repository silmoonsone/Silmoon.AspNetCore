using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Razor
{
    public class RazorComponentHelper
    {
        public static async Task<byte[]> ReadInputFile(InputFileChangeEventArgs e, Func<long, long, decimal, Task> reading, Func<byte[], long, Task> finish, int bufferSize = 1024 * 512)
        {
            var stream = e.File.OpenReadStream(e.File.Size);
            var dataList = new List<byte>();
            var buff = new byte[bufferSize];
            do
            {
                var recvLen = await stream.ReadAsync(buff);
                dataList.AddRange(buff.Take(recvLen));
                decimal percent = decimal.Divide(dataList.Count, e.File.Size);
                if (reading != null) await reading.Invoke(e.File.Size, dataList.Count, percent);
            } while (dataList.Count < stream.Length);

            byte[] data = [.. dataList];
            if (finish != null) await finish.Invoke(data, e.File.Size);
            return data;
        }
    }
}
