using Newtonsoft.Json;

namespace Silmoon.AspNetCore.Encryption.Models
{
    public class VerifyWebAuthnResponse
    {
        [JsonProperty("rawId")]
        public byte[] RawId { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("response")]
        public VerifyWebAuthnResponseResponse Response { get; set; }

        public class VerifyWebAuthnResponseResponse
        {
            [JsonProperty("authenticatorData")]
            public byte[] AuthenticatorData { get; set; }
            [JsonProperty("clientDataJSON")]
            public byte[] ClientDataJSON { get; set; }
            [JsonProperty("signature")]
            public byte[] Signature { get; set; }
        }
    }
}
