using Newtonsoft.Json;
using Silmoon.Extension;

namespace Silmoon.AspNetCore.Encryption.Models
{
    public class BlazorCreateWebAuthnKeyResponse : CreateWebAuthnKeyResponse
    {
        [JsonIgnore]
        public AttestationObjectData AttestationObjectData { get; set; }
    }
}