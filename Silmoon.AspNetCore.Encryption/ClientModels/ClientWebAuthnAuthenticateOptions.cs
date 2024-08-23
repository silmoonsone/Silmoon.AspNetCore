using Newtonsoft.Json;

namespace Silmoon.AspNetCore.Encryption.ClientModels
{
    public class ClientWebAuthnAuthenticateOptions
    {
        [JsonProperty("challenge")]
        public byte[] Challenge { get; set; }
        [JsonProperty("rpId")]
        public string RpId { get; set; }
        [JsonProperty("allowCredentials")]
        public Credential[] AllowCredentials { get; set; }
        [JsonProperty("timeout")]
        public int Timeout { get; set; } = 60000;
        [JsonProperty("userVerification")]
        public string UserVerification { get; set; } = "preferred";
    }
}
