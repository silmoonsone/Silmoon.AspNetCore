using Newtonsoft.Json;

namespace Silmoon.AspNetCore.Encryption.ClientModels
{
    public class ClientBlazorWebAuthnAuthenticateOptions
    {
        /// <summary>
        /// 用于验证的挑战字符串，通常是一个随机生成的字符串。
        /// </summary>
        [JsonProperty("challenge")]
        required public string Challenge { get; set; }
        /// <summary>
        /// 当前认证请求的Relying Party (RP) ID，通常是域名或应用名称。
        /// </summary>
        [JsonProperty("rpId", NullValueHandling = NullValueHandling.Ignore)]
        required public string RpId { get; set; }
        [JsonProperty("allowCredentials")]
        public Credential[] AllowCredentials { get; set; } = [];
        [JsonProperty("timeout")]
        public int Timeout { get; set; } = 60000;
        [JsonProperty("userVerification")]
        public string UserVerification { get; set; } = "preferred";
    }
}
