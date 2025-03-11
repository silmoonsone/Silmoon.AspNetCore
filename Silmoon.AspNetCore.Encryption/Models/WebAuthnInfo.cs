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
        private static readonly Dictionary<string, string> aguidMapping = new Dictionary<string, string>
        {
            { "08987058cadc4b81b6e130de50dcbe96", "Windows Hello" },
            { "9ddd1817af5a4672a2b93e3dd95000a9", "Windows Hello" },
            { "6028b017b1d44c02b4b3afcdafc96bb2", "Windows Hello" },
            { "dd4ec289e01d41c9bb8970fa845d4bf2", "iCloud Keychain" },
            { "fbfc3007154e4ecc8c0b6e020557d7bd", "iCloud Keychain" },
            { "ea9b8d664d011d213ce4b6b48cb575d4", "Google Password Manager" },

            { "73e5fe520a9e4f0c8f043ee66015e232", "Apple Face ID / Touch ID" },
            { "00000000000000000000000000000000", "Anonymous Authenticator" },
            { "2fc0579f811347eab116bb5a8db9202a", "YubiKey 5 NFC (Firmware 5.2, 5.4)" },
            { "fa2b99dc9e3942578f924a30d23c4118", "YubiKey 5 NFC (Firmware 5.1)" },
            { "cb69481e8ff7403993ec0a2729a154a8", "YubiKey 5 (USB-A, No NFC, Firmware 5.1)" },
            { "ee882879721c491397753dfcce97072a", "YubiKey 5 (USB-A, No NFC, Firmware 5.2, 5.4)" },
            { "c5ef55ffad9a4b9fb580adebafe026d0", "YubiKey 5Ci (Firmware 5.2, 5.4)" },
            { "f8a011f38c0a4d15800617111f9edc7d", "Security Key by Yubico (Blue, Firmware 5.1)" },
            { "b92c3f9ac0144056887f140a2501163b", "Security Key by Yubico (Blue, Firmware 5.2)" },
            { "6d44ba9bf6ec2e49b9300c8fe920cb73", "Security Key NFC (Blue, Firmware 5.1)" },
            { "149a20218ef6413396b881f8d5b7f1f5", "Security Key NFC (USB-A, USB-C, Blue, Firmware 5.2, 5.4)" },
            { "d8522d9f575b486688a9ba99fa02f35b", "YubiKey Bio - FIDO Edition (Firmware 5.5, 5.6)" },
            { "a4e9fc6d4cbe4758b8ba37598bb5bbaa", "Security Key NFC (Black, USB-A, USB-C, Firmware 5.4)" },
            { "0bb43545fd2c418587ddfeb0b2916ace", "Security Key NFC - Enterprise Edition (USB-A, USB-C, Black, Firmware 5.4)" },
            { "7e3f3d3035574442bdae139312178b39", "RSA FIDO CA Root" },
            { "1ac71f64468d4fe0bef10e5f2f551f18", "YubiKey 5 NFC (Enterprise Attestation, Firmware 5.7)" },
            { "20ac7a17c814483393fe539f0d5e3389", "YubiKey 5C Nano (Enterprise Attestation, Firmware 5.7)" },
            { "b90e7dc1316e4feea25a56a666a670fe", "YubiKey 5Ci (Enterprise Attestation, Firmware 5.7)" },
            { "8c39ee867f9a4a959ba3f6b097e5c2ee", "YubiKey Bio Series - FIDO Edition (Enterprise Attestation, Firmware 5.7)" },
            { "97e6a830c952474095fc7c78dc97ce47", "YubiKey Bio Series - Multi-protocol Edition (Enterprise Attestation, Firmware 5.7)" },
            { "9ff4cc6561544fffba099e2af7882ad2", "Security Key NFC - Enterprise Edition (USB-A, USB-C, Black, Firmware 5.7)" }
        };

        public string AAGUID { get; set; }
        public string AttestationFormat { get; set; }
        public byte[] CredentialId { get; set; }
        public int SignCount { get; set; }
        public bool UserVerified { get; set; }
        public string AuthenticatorAttachment { get; set; }
        public byte[] AttestationObject { get; set; }

        public string GetDeviceDescription() => aguidMapping.ContainsKey(AAGUID) ? aguidMapping[AAGUID] : "Unknown";

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

        public override string ToString() => CredentialId.GetBase64String();
    }
}
