using System.Formats.Asn1;
using System;

namespace Silmoon.AspNetCore.Encryption.Components
{
    public class AlgorithmHelper
    {
        public static byte[] ConvertToSubjectPublicKeyInfo(byte[] x, byte[] y)
        {
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
    }
}
