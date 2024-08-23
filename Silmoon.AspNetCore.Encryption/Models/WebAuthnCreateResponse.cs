using Newtonsoft.Json;

namespace Silmoon.AspNetCore.Encryption.Models
{
    /// <summary>
    /// 在创建过程中客户端返回的创建WebAuthn的响应经过Json解析后的对象
    /// </summary>
    public class WebAuthnCreateResponse
    {
        [JsonProperty("rawId")]
        public byte[] RawId { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("authenticatorAttachment")]
        public string AuthenticatorAttachment { get; set; }
        [JsonProperty("response")]
        public CreateWebAuthnResponse Response { get; set; }

        public class CreateWebAuthnResponse
        {
            [JsonProperty("attestationObject")]
            public byte[] AttestationObject { get; set; }
            [JsonProperty("clientDataJSON")]
            public byte[] ClientDataJson { get; set; }
        }
    }
}