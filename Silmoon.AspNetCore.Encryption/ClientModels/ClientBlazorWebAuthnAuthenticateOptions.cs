using Newtonsoft.Json;

namespace Silmoon.AspNetCore.Encryption.ClientModels
{
    public class ClientBlazorWebAuthnAuthenticateOptions
    {
        [JsonProperty("challenge")]
        required public string Challenge { get; set; }
        [JsonProperty("rpId")]
        required public string RpId { get; set; }
        [JsonProperty("allowCredentials")]
        public Credential[] AllowCredentials { get; set; } = [];
        [JsonProperty("timeout")]
        public int Timeout { get; set; } = 60000;
        [JsonProperty("userVerification")]
        public string UserVerification { get; set; } = "preferred";
    }
}
