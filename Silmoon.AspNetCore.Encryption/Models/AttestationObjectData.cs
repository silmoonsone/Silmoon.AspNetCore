using System.Collections.Generic;

namespace Silmoon.AspNetCore.Encryption.Models
{
    public class AttestationObjectData
    {
        public byte[] AuthenticatorData { get; set; }
        public string AAGUID { get; set; }
        public string AttestationFormat { get; set; }
        public Dictionary<string, object> AttestationStatement { get; set; }
        public byte[] CredentialId { get; set; }
        public string PublicKeyAlgorithm { get; set; }
        public byte[] PublicKey { get; set; }
        public int SignCount { get; set; }
        public bool UserVerified { get; set; }
    }
}
