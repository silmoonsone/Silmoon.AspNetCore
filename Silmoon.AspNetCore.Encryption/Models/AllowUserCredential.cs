using Newtonsoft.Json;
using Silmoon.AspNetCore.Encryption.ClientModels;
using System.Collections.Generic;

namespace Silmoon.AspNetCore.Encryption.Models
{
    public class AllowUserCredential
    {
        public string UserId { get; set; }
        public Credential[] Credentials { get; set; }
    }
}
