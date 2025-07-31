using Silmoon.AspNetCore.Encryption.Enums;
using System.Collections.Generic;

namespace Silmoon.AspNetCore.Encryption.Models
{
    /// <summary>
    /// 从客户端返回的AttestationObject经过解析后的数据
    /// </summary>
    public class AttestationObjectData
    {
        public byte[] AuthenticatorData { get; set; }
        public string AAGUID { get; set; }
        public string AttestationFormat { get; set; }
        public Dictionary<string, object> AttestationStatement { get; set; }
        public byte[] CredentialId { get; set; }
        public WebAuthnPublicKeyAlgorithm PublicKeyAlgorithm { get; set; }
        public byte[] PublicKey { get; set; }
        public int SignCount { get; set; }
        public bool UserVerified { get; set; }
    }
}
