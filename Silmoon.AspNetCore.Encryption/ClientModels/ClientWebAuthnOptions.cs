using Newtonsoft.Json;
using System.Collections.Generic;

namespace Silmoon.AspNetCore.Encryption.ClientModels
{
    public class ClientWebAuthnOptions
    {
        [JsonProperty("challenge")]
        required public byte[] Challenge { get; set; }
        [JsonProperty("rp")]
        required public ClientWebAuthnRp Rp { get; set; }
        [JsonProperty("user")]
        required public ClientWebAuthnUser User { get; set; }

        [JsonProperty("pubKeyCredParams")]
        public List<ClientWebAuthnPubKeyCredParams> PubKeyCredParams { get; set; } = new List<ClientWebAuthnPubKeyCredParams>()
        {
            new ClientWebAuthnPubKeyCredParams() { Alg = -7, Type = "public-key" },
            new ClientWebAuthnPubKeyCredParams() { Alg = -257, Type = "public-key" }
        };
        [JsonProperty("authenticatorSelection")]
        required public ClientWebAuthnAuthenticatorSelection AuthenticatorSelection { get; set; } = new ClientWebAuthnAuthenticatorSelection();
        [JsonProperty("timeout")]
        public int Timeout { get; set; } = 60000;
        [JsonProperty("attestation")]
        public string Attestation { get; set; } = "direct";


        public class ClientWebAuthnRp
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
            public string Id { get; set; }
        }
        public class ClientWebAuthnUser
        {
            [JsonProperty("id")]
            required public byte[] Id { get; set; }
            [JsonProperty("name")]
            required public string Name { get; set; }
            [JsonProperty("displayName")]
            required public string DisplayName { get; set; }
        }
        public class ClientWebAuthnPubKeyCredParams
        {
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("alg")]
            public int Alg { get; set; }
        }
        public class ClientWebAuthnAuthenticatorSelection
        {
            [JsonProperty("authenticatorAttachment")]
            public string AuthenticatorAttachment { get; set; }
            [JsonProperty("userVerification")]
            public string UserVerification { get; set; } = "preferred";
        }
    }
}
