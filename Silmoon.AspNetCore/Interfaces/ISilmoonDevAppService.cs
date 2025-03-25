using Silmoon.Models;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Interfaces
{
    /// <summary>
    /// 为应用程序提供AppId和Key
    /// </summary>
    public interface ISilmoonDevAppService
    {
        Task<StateSet<bool, (string SignatureKey, string EncryptKey)>> GetKey(string AppId);
        Task<StateSet<bool, (string SignatureKey, string EncryptKey)>> GetCachedKey(string AppId);
    }
}
