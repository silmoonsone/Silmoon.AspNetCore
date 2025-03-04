using LiteDB;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.Data.LiteDB.Converters;
using Silmoon.Data.LiteDB.Interfaces;
using Silmoon.Extension;

namespace Silmoon.AspNetCore.UserAuthTest.Models.SubModels
{
    public class UserWebAuthnInfo : WebAuthnInfo, IIdObject, ICreatedAt
    {
        [Newtonsoft.Json.JsonConverter(typeof(ObjectIdJsonConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(ObjectIdStringJsonConverter))]
        public ObjectId _id { get; set; } = ObjectId.NewObjectId();
        [Newtonsoft.Json.JsonConverter(typeof(ObjectIdJsonConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(ObjectIdStringJsonConverter))]
        public ObjectId UserObjectId { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return CredentialId.GetBase64String();
        }
    }
}
