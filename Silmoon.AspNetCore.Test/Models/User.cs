using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Silmoon.Data.MongoDB.Converters;
using Silmoon.Data.MongoDB.Interfaces;
using Silmoon.Extension.Interfaces;
using Silmoon.Extension.Enums;

namespace Silmoon.AspNetCore.Test.Models
{
    public class User : IDefaultUserIdentity, IIdObject, ICreatedAt
    {
        [Newtonsoft.Json.JsonConverter(typeof(ObjectIdJsonConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(ObjectIdStringJsonConverter))]
        public ObjectId _id { get; set; } = ObjectId.GenerateNewId();

        public string Username { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
        public IdentityRole Role { get; set; }
        public IdentityStatus Status { get; set; }


        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime created_at { get; set; } = DateTime.Now;
    }
}
