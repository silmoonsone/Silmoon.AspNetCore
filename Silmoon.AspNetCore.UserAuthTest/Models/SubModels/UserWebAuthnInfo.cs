using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Silmoon.AspNetCore.Encryption.Models;
using Silmoon.Data.MongoDB.Converters;
using Silmoon.Data.MongoDB.Models;
using Silmoon.Extension;

namespace Silmoon.AspNetCore.UserAuthTest.Models.SubModels
{
    public class UserWebAuthnInfo : WebAuthnInfo, IIdObject, ICreatedAt
    {
        [Newtonsoft.Json.JsonConverter(typeof(ObjectIdJsonConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(ObjectIdStringJsonConverter))]
        public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
        [Newtonsoft.Json.JsonConverter(typeof(ObjectIdJsonConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(ObjectIdStringJsonConverter))]
        public ObjectId UserObjectId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime created_at { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return CredentialId.GetBase64String();
        }
    }
}
