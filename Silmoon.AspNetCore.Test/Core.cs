using MongoDB.Bson;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.AspNetCore.Services.Interfaces;
using Silmoon.AspNetCore.Test.Models;
using Silmoon.AspNetCore.Test.Services;
using Silmoon.Data.LiteDB;
using Silmoon.Data.MongoDB;
using Silmoon.Extension;
using Silmoon.Models;
using System.Formats.Asn1;

namespace Silmoon.AspNetCore.Test
{
    public class Core : MongoService
    {
        public override MongoExecuter Executer { get; set; }
        public SilmoonConfigureServiceImpl SilmoonConfigureService { get; set; }
        public Core(ISilmoonConfigureService silmoonConfigureService)
        {
            SilmoonConfigureService = (SilmoonConfigureServiceImpl)silmoonConfigureService;
            Executer = new MongoExecuter(SilmoonConfigureService.MongoDBConnectionString);
        }

        public User GetUser(ObjectId UserObjectId) => Get<User>(x => x._id == UserObjectId);
        public User GetUser(string username) => Get<User>(x => x.Username == username);
        public StateSet<bool> NewUser(User user)
        {
            if (GetUser(user.Username) is null)
            {
                Add(user);
                return true.ToStateSet();
            }
            else
                return false.ToStateSet("User already exists");
        }
        public UserAuthInfo GetUserAuthInfo(ObjectId UserObjectId)
        {
            if (GetUser(UserObjectId) is null) return null;
            var result = Get<UserAuthInfo>(x => x.UserObjectId == UserObjectId);
            if (result is null)
            {
                result = new UserAuthInfo()
                {
                    UserObjectId = UserObjectId,
                    WebAuthnInfos = []
                };
                Add(result);
            }
            return result;
        }
        public WebAuthnInfo[] GetUserWebAuthnInfos(ObjectId UserObjectId)
        {
            return GetUserAuthInfo(UserObjectId)?.WebAuthnInfos.ToArray();
        }

        public WebAuthnInfo GetUserWebAuthnInfo(ObjectId UserObjectId, byte[] CredentialId)
        {
            var userAuthInfo = GetUserAuthInfo(UserObjectId);
            return userAuthInfo.WebAuthnInfos.FirstOrDefault(x => x.CredentialId != null && x.CredentialId.SequenceEqual(CredentialId));
        }
        public StateSet<bool> AddUserWebAuthnInfo(ObjectId UserObjectId, WebAuthnInfo userWebAuthnInfo)
        {
            var userAuthInfo = GetUserAuthInfo(UserObjectId);
            if (GetUserWebAuthnInfo(UserObjectId, userWebAuthnInfo.CredentialId) is null)
            {
                userAuthInfo.WebAuthnInfos.Add(userWebAuthnInfo);
                Sets(new UserAuthInfo() { WebAuthnInfos = userAuthInfo.WebAuthnInfos }, x => x.UserObjectId == UserObjectId, null, x => x.WebAuthnInfos);
                return true.ToStateSet();
            }
            else
                return false.ToStateSet("CredentialId already exists");
        }
        public StateSet<bool> DeleteUserWebAuthnInfo(ObjectId UserObjectId, byte[] CredentialId)
        {
            var userAuthInfo = GetUserAuthInfo(UserObjectId);
            var userWebAuthnInfo = GetUserWebAuthnInfo(UserObjectId, CredentialId);
            if (userWebAuthnInfo is null)
                return false.ToStateSet("CredentialId not found");
            else
            {
                userAuthInfo.WebAuthnInfos.Remove(userAuthInfo.WebAuthnInfos.Where(x => x.CredentialId != null && x.CredentialId.SequenceEqual(CredentialId)).FirstOrDefault());
                Sets(new UserAuthInfo() { WebAuthnInfos = userAuthInfo.WebAuthnInfos }, x => x.UserObjectId == UserObjectId, null, x => x.WebAuthnInfos);
                return true.ToStateSet();
            }
        }


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
