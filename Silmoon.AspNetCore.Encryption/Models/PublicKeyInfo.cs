using Silmoon.AspNetCore.Encryption.Enums;

namespace Silmoon.AspNetCore.Encryption.Models
{
    /// <summary>
    /// 在用户验证过程中向WebAuthnService实现获取用户公钥提供的信息
    /// </summary>
    public class PublicKeyInfo
    {
        public byte[] PublicKey { get; set; }
        public WebAuthnPublicKeyAlgorithm PublicKeyAlgorithm { get; set; }
    }
}
