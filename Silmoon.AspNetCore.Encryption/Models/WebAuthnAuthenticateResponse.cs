using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silmoon.Extension;
using Silmoon.Models;
using System.Security.Cryptography;

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



        public byte[] SignedData
        {
            get => [.. Response.AuthenticatorData, .. SHA256.HashData(Response.ClientDataJson)];
        }
        public StateSet<bool> VerifySignal(PublicKeyInfo publicKeyInfo)
        {
            if (publicKeyInfo.PublicKeyAlgorithm == "ES256")
            {
                using var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(publicKeyInfo.PublicKey, out _);
                return true.ToStateSet();
            }
            else if (publicKeyInfo.PublicKeyAlgorithm == "RS256")
            {
                var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(publicKeyInfo.PublicKey, out _);
                return true.ToStateSet();
            }
            else
                return false.ToStateSet("Unsupported public key algorithm");
        }
        public StateSet<bool> VerifySignal(byte[] publicKey, string publicKeyAlgorithm)
        {
            if (publicKeyAlgorithm == "ES256")
            {
                using var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
                return true.ToStateSet();
            }
            else if (publicKeyAlgorithm == "RS256")
            {
                var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(publicKey, out _);
                return true.ToStateSet();
            }
            else
                return false.ToStateSet("Unsupported public key algorithm");
        }

    }
}
