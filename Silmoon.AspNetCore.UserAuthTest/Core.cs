using LiteDB;
using Silmoon.AspNetCore.UserAuthTest.Models;
using Silmoon.AspNetCore.UserAuthTest.Models.SubModels;
using Silmoon.Data.LiteDB;
using Silmoon.Extension;
using Silmoon.Models;

namespace Silmoon.AspNetCore.UserAuthTest
{
    public class Core : LiteDBService
    {
        public Core()
        {
            Database = new LiteDatabase("test.local.db");
        }
        public StateSet<bool> AddUser(User user)
        {
            if (GetUser(user.Username) is null)
            {
                Add(user);
                return true.ToStateSet("user sign up success.");
            }
            else
            {
                return false.ToStateSet("user is exists");
            }
        }

        public User GetUser(ObjectId id) => Get<User>(x => x._id == id);
        public User GetUser(string username) => Get<User>(x => x.Username == username);
        public void AddUserWebAuthnInfo(UserWebAuthnInfo userWebAuthnInfo)
        {
            Add(userWebAuthnInfo);
        }
        public int DeleteUserWebAuthnInfo(ObjectId UserObjectId, byte[] CredentialId)
        {
            return Deletes<UserWebAuthnInfo>(x => x.CredentialId == CredentialId && x.UserObjectId == UserObjectId);
        }

        public UserWebAuthnInfo[] GetUserWebAuthnInfos(ObjectId UserObjectId)
        {
            return Gets<UserWebAuthnInfo>(x => x.UserObjectId == UserObjectId).ToArray();
        }
        public UserWebAuthnInfo GetUserWebAuthnInfo(byte[] CredentialId)
        {
            return Get<UserWebAuthnInfo>(x => x.CredentialId == CredentialId);
        }

    }
}
