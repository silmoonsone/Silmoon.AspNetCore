using Newtonsoft.Json;

namespace Silmoon.AspNetCore.Encryption.ClientModels
{
    public class ClientBlazorWebAuthnOptions
    {
        /// <summary>
        /// 一个用于挑战的随机字符串。
        /// </summary>
        [JsonProperty("challenge")]
        required public string Challenge { get; set; }
        [JsonProperty("rp")]
        required public ClientWebAuthnOptions.ClientWebAuthnRp Rp { get; set; }
        [JsonProperty("user")]
        required public ClientWebAuthnUser User { get; set; }

        [JsonProperty("pubKeyCredParams")]
        public List<ClientWebAuthnOptions.ClientWebAuthnPubKeyCredParams> PubKeyCredParams { get; set; } =
        [
            new ClientWebAuthnOptions.ClientWebAuthnPubKeyCredParams() { Alg = -7, Type = "public-key" },
            new ClientWebAuthnOptions.ClientWebAuthnPubKeyCredParams() { Alg = -257, Type = "public-key" }
        ];
        [JsonProperty("authenticatorSelection")]
        public ClientWebAuthnOptions.ClientWebAuthnAuthenticatorSelection AuthenticatorSelection { get; set; } = new ClientWebAuthnOptions.ClientWebAuthnAuthenticatorSelection();
        [JsonProperty("timeout")]
        public int Timeout { get; set; } = 60000;
        [JsonProperty("attestation")]
        public string Attestation { get; set; } = "direct";


        public class ClientWebAuthnUser
        {
            /// <summary>
            /// 用户的唯一标识符，通常是用户的ID。
            /// </summary>
            [JsonProperty("id")]
            required public string Id { get; set; }
            /// <summary>
            /// 用户的名称，通常是用户名或电子邮件地址。
            /// </summary>
            [JsonProperty("name")]
            required public string Name { get; set; }
            /// <summary>
            /// 用户的显示名称，通常是用户的全名或昵称。
            /// </summary>
            [JsonProperty("displayName")]
            required public string DisplayName { get; set; }
        }
    }
}
