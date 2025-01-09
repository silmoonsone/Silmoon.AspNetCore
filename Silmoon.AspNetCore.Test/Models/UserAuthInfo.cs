using MongoDB.Bson;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.Data.MongoDB;

namespace Silmoon.AspNetCore.Test.Models
{
    public class UserAuthInfo : IdObject
    {
        public ObjectId UserObjectId { get; set; }
        public List<WebAuthnInfo> WebAuthnInfos { get; set; } = [];

    }
}
