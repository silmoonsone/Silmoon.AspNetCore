using Newtonsoft.Json;
using System.Collections.Generic;

namespace Silmoon.AspNetCore.Encryption.ClientModels
{
    public class ClientBlazorWebAuthnOptions
    {
        /// <summary>
        /// 将byte[]转换成Base64字符串。
        /// </summary>
        [JsonProperty("challenge")]
        required public string Challenge { get; set; }
        [JsonProperty("rp")]
        required public ClientWebAuthnOptions.ClientWebAuthnRp Rp { get; set; }
        [JsonProperty("user")]
        required public ClientWebAuthnUser User { get; set; }

        [JsonProperty("pubKeyCredParams")]
        public List<ClientWebAuthnOptions.ClientWebAuthnPubKeyCredParams> PubKeyCredParams { get; set; } = new List<ClientWebAuthnOptions.ClientWebAuthnPubKeyCredParams>()
        {
            new ClientWebAuthnOptions.ClientWebAuthnPubKeyCredParams() { Alg = -7, Type = "public-key" },
            new ClientWebAuthnOptions.ClientWebAuthnPubKeyCredParams() { Alg = -257, Type = "public-key" }
        };
        [JsonProperty("authenticatorSelection")]
        required public ClientWebAuthnOptions.ClientWebAuthnAuthenticatorSelection AuthenticatorSelection { get; set; } = new ClientWebAuthnOptions.ClientWebAuthnAuthenticatorSelection();
        [JsonProperty("timeout")]
        public int Timeout { get; set; } = 60000;
        [JsonProperty("attestation")]
        public string Attestation { get; set; } = "direct";


        public class ClientWebAuthnUser
        {
            /// <summary>
            /// 将byte[]转换成Base64字符串。
            /// </summary>
            [JsonProperty("id")]
            required public string Id { get; set; }
            [JsonProperty("name")]
            required public string Name { get; set; }
            [JsonProperty("displayName")]
            required public string DisplayName { get; set; }
        }
    }
}
