using Silmoon.AspNetCore.UserAuthTest.Models.SubModels;
using Silmoon.Data.LiteDB.Models;
using Silmoon.Extensions.Enums;
using Silmoon.Extensions.Interfaces;

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
