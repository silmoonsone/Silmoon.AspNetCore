using PeterO.Cbor;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.Extension;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Security.Cryptography;

namespace Silmoon.AspNetCore.Encryption
{
    public static class WebAuthnParser
    {
        public static AttestationObjectData ParseAttestationObject(byte[] attestationObjectByteArray)
        {
            var attestationObject = CBORObject.DecodeFromBytes(attestationObjectByteArray);

            // 提取 authData (byte array)
            byte[] authData = attestationObject["authData"].GetByteString();

            // 提取 AAGUID (16字节)
            string aaguid = BitConverter.ToString(authData.Skip(37).Take(16).ToArray()).Replace("-", "").ToLower();

            // 提取 Credential ID 长度和值
            int credIdLen = BitConverter.ToUInt16(authData.Skip(53).Take(2).Reverse().ToArray(), 0);
            byte[] credentialId = authData.Skip(55).Take(credIdLen).ToArray();

            // 提取 COSE_Key 数据，从 Credential ID 后的位置开始
            int keyOffset = 55 + credIdLen;
            byte[] coseKey = authData.Skip(keyOffset).ToArray();

            var cborKey = CBORObject.DecodeFromBytes(coseKey);

            // 提取公钥算法
            string publicKeyAlgorithm = ExtractPublicKeyAlgorithm(cborKey);

            // 提取公钥
            byte[] publicKey = ExtractPublicKey(cborKey, publicKeyAlgorithm);

            // 提取用户验证状态和签名计数器
            bool userVerified = (authData[32] & 0x04) != 0;
            int signCount = BitConverter.ToInt32(authData.Skip(33).Take(4).Reverse().ToArray(), 0);

            // 提取 Attestation Statement
            var attStmt = new Dictionary<string, object>();
            foreach (var key in attestationObject["attStmt"].Keys)
            {
                attStmt.Add(key.ToString(), attestationObject["attStmt"][key]);
            }

            return new AttestationObjectData
            {
                AuthenticatorData = authData,
                AAGUID = aaguid,
                CredentialId = credentialId,
                PublicKeyAlgorithm = publicKeyAlgorithm,
                PublicKey = publicKey,
                SignCount = signCount,
                UserVerified = userVerified,
                AttestationFormat = attestationObject["fmt"].AsString(),
                AttestationStatement = attStmt
            };
        }

        private static string ExtractPublicKeyAlgorithm(CBORObject cborKey)
        {
            int alg = cborKey[CBORObject.FromObject(3)].AsInt32();
            return alg switch
            {
                -7 => "ES256",  // ECDSA w/ SHA-256
                -257 => "RS256", // RSASSA-PKCS1-v1_5 w/ SHA-256
                _ => "Unknown"
            };
        }

        private static byte[] ExtractPublicKey(CBORObject cborKey, string algorithm)
        {
            if (algorithm == "ES256")
            {
                // 提取 EC2 公钥（适用于 ES256）
                byte[] x = cborKey[CBORObject.FromObject(-2)].GetByteString();  // X coordinate
                byte[] y = cborKey[CBORObject.FromObject(-3)].GetByteString();  // Y coordinate
                // Create the EC point format (0x04 || X || Y) for uncompressed format
                byte[] ecPoint = new byte[1 + x.Length + y.Length];
                ecPoint[0] = 0x04; // Uncompressed point indicator
                Buffer.BlockCopy(x, 0, ecPoint, 1, x.Length);
                Buffer.BlockCopy(y, 0, ecPoint, 1 + x.Length, y.Length);

                var writer = new AsnWriter(AsnEncodingRules.DER);

                writer.PushSequence();

                // AlgorithmIdentifier SEQUENCE
                writer.PushSequence();
                writer.WriteObjectIdentifier("1.2.840.10045.2.1"); // id-ecPublicKey OID
                writer.WriteObjectIdentifier("1.2.840.10045.3.1.7"); // prime256v1 OID
                writer.PopSequence();

                // PublicKey BIT STRING
                writer.WriteBitString(ecPoint);

                writer.PopSequence();

                return writer.Encode();
            }
            else if (algorithm == "RS256")
            {
                byte[] n = cborKey[CBORObject.FromObject(-1)].GetByteString();  // Modulus
                byte[] e = cborKey[CBORObject.FromObject(-2)].GetByteString();  // Exponent
                using var rsa = RSA.Create();
                var rsaParameters = new RSAParameters { Modulus = n, Exponent = e };
                rsa.ImportParameters(rsaParameters);
                return rsa.ExportRSAPublicKey();  // 导出 X.509 格式的公钥并存储
            }
            throw new NotSupportedException("Unsupported public key algorithm.");
        }

        public static byte[] ConvertDerToRS(ReadOnlySpan<byte> derSignature)
        {
            // 确保签名是 DER 编码格式
            if (derSignature.Length < 8 || derSignature[0] != 0x30) throw new ArgumentException("Invalid DER signature format.", nameof(derSignature));

            int offset = 2; // 跳过 SEQUENCE 标签和长度

            // 如果长度使用多字节表示，则调整 offset
            if (derSignature[1] > 0x80) offset += derSignature[1] - 0x80;

            // 获取 r 的长度
            int rLength = derSignature[offset + 1];
            var r = derSignature.Slice(offset + 2, rLength);

            // 如果 r 是 33 字节并且第一个字节是 0x00，去掉前导字节
            if (r.Length == 33 && r[0] == 0x00)
                r = r.Slice(1); // 切掉第一个字节

            // 获取 s 的位置和长度
            int sOffset = offset + 2 + rLength;
            int sLength = derSignature[sOffset + 1];
            var s = derSignature.Slice(sOffset + 2, sLength);

            // 如果 s 是 33 字节并且第一个字节是 0x00，去掉前导字节
            if (s.Length == 33 && s[0] == 0x00)
                s = s.Slice(1); // 切掉第一个字节

            // 创建一个长度为 64 字节的数组用于存放结果
            byte[] result = new byte[64];

            // 将 r 和 s 拷贝到结果数组中
            r.CopyTo(result.AsSpan(0, 32));
            s.CopyTo(result.AsSpan(32, 32));

            return result;
        }

    }
}
