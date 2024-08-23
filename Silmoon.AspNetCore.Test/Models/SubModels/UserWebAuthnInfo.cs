using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.Extension;

namespace Silmoon.AspNetCore.Test.Models.SubModels
{
    public class UserWebAuthnInfo : PublicKeyInfo
    {
        public string AAGuid { get; set; }
        public string AttestationFormat { get; set; }
        public byte[] CredentialId { get; set; }
        public int SignCount { get; set; }
        public bool UserVerified { get; set; }
        public string AuthenticatorAttachment { get; set; }
        public byte[] AttestationObject { get; set; }

        public override string ToString()
        {
            return CredentialId.GetBase64String();
        }
    }
}
