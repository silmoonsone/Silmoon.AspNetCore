using Newtonsoft.Json;
using Silmoon.Extension;

namespace Silmoon.AspNetCore.Encryption.Models
{
    public class BlazorWebAuthnCreateResponse : WebAuthnCreateResponse
    {
        [JsonIgnore]
        public AttestationObjectData AttestationObjectData { get; set; }
    }
}