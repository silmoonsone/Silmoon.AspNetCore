using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silmoon.Extension;

namespace Silmoon.AspNetCore.Encryption.Models
{
    public class WebAuthnAuthenticateResponse
    {
        [JsonProperty("rawId")]
        public byte[] RawId { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("response")]
        public VerifyWebAuthnResponseResponse Response { get; set; }

        public class VerifyWebAuthnResponseResponse
        {
            JObject clientJson;
            [JsonProperty("authenticatorData")]
            public byte[] AuthenticatorData { get; set; }
            [JsonProperty("clientDataJSON")]
            public byte[] ClientDataJson { get; set; }
            [JsonProperty("signature")]
            public byte[] Signature { get; set; }
            public JObject GetClientJson()
            {
                return clientJson ??= JObject.Parse(ClientDataJson.GetString());
            }
            public string Challenge => GetClientJson()["challenge"].Value<string>().Base64UrlToBase64();
        }
    }
}
