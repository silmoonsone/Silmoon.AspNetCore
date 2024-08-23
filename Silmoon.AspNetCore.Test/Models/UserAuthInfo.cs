using MongoDB.Bson;
using Silmoon.AspNetCore.Test.Models.SubModels;
using Silmoon.Data.MongoDB;

namespace Silmoon.AspNetCore.Test.Models
{
    public class UserAuthInfo : IdObject
    {
        public ObjectId UserObjectId { get; set; }
        public List<UserWebAuthnInfo> WebAuthnInfos { get; set; } = [];

    }
}
