using MongoDB.Bson;
using Silmoon.AspNetCore.UserAuthTest.Models.SubModels;
using Silmoon.Data.MongoDB;
using Silmoon.Data.MongoDB.Converters;
using Silmoon.Extension.Models.Identities;
using Silmoon.Extension.Models.Identities.Enums;

namespace Silmoon.AspNetCore.UserAuthTest.Models
{
    public class User : IdObject, IDefaultUserIdentity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
        public IdentityRole Role { get; set; }
        public IdentityStatus Status { get; set; }
    }
}
