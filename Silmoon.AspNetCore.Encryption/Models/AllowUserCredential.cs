using Newtonsoft.Json;
using Silmoon.AspNetCore.Encryption.ClientModels;
using System.Collections.Generic;

namespace Silmoon.AspNetCore.Encryption.Models
{
    /// <summary>
    /// 返回给客户端的表示可接受的用户的WebAuthn的列表的单个信息
    /// </summary>
    public class AllowUserCredential
    {
        public string UserId { get; set; }
        public Credential[] Credentials { get; set; }
    }
}
