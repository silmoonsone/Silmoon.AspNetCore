using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Encryption.Enums
{
    public enum WebAuthnPublicKeyAlgorithm
    {
        RS256 = -257, // RSA with SHA-256
        ES256 = -7, // ECDSA with SHA-256
        None = 0,
    }
}
