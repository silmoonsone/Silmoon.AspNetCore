using Newtonsoft.Json;
using Silmoon.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Encryption.Models
{
    public class WebAuthnInfo : PublicKeyInfo
    {
        public string AAGUID { get; set; }
        public string AttestationFormat { get; set; }
        public byte[] CredentialId { get; set; }
        public int SignCount { get; set; }
        public bool UserVerified { get; set; }
        public string AuthenticatorAttachment { get; set; }
        public byte[] AttestationObject { get; set; }

        public static WebAuthnInfo Create(WebAuthnCreateResponse response)
        {
            return new WebAuthnInfo
            {
                AAGUID = response.AttestationObjectData.AAGUID,
                AttestationFormat = response.AttestationObjectData.AttestationFormat,
                CredentialId = response.AttestationObjectData.CredentialId,
                PublicKey = response.AttestationObjectData.PublicKey,
                PublicKeyAlgorithm = response.AttestationObjectData.PublicKeyAlgorithm,
                SignCount = response.AttestationObjectData.SignCount,
                UserVerified = response.AttestationObjectData.UserVerified,
                AttestationObject = response.Response.AttestationObject,
                AuthenticatorAttachment = response.AuthenticatorAttachment,
            };
        }
        public static WebAuthnInfo Create(AttestationObjectData attestationObjectData, byte[] attestationObjectByteArray, string authenticatorAttachment)
        {
            return new WebAuthnInfo
            {
                AAGUID = attestationObjectData.AAGUID,
                AttestationFormat = attestationObjectData.AttestationFormat,
                CredentialId = attestationObjectData.CredentialId,
                PublicKey = attestationObjectData.PublicKey,
                PublicKeyAlgorithm = attestationObjectData.PublicKeyAlgorithm,
                SignCount = attestationObjectData.SignCount,
                UserVerified = attestationObjectData.UserVerified,
                AttestationObject = attestationObjectByteArray,
                AuthenticatorAttachment = authenticatorAttachment,
            };
        }

        public override string ToString()
        {
            return CredentialId.GetBase64String();
        }
    }
}
