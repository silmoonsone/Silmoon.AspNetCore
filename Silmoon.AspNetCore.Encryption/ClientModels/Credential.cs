using Newtonsoft.Json;

namespace Silmoon.AspNetCore.Encryption.ClientModels
{
    public class Credential
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; } = "public-key";
    }
}
