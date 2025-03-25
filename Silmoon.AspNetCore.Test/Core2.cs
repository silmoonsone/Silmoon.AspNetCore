using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Test.Models;
using Silmoon.AspNetCore.Test.Services;
using Silmoon.AspNetCore.Interfaces;
using Silmoon.Data.MongoDB;
using Silmoon.Extension;
using Silmoon.Models;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Silmoon.Secure;

namespace Silmoon.AspNetCore.Test;

public class Core2 : MongoService, IDisposable
{
    public override MongoExecuter Executer { get; set; }
    public SilmoonConfigureServiceImpl SilmoonConfigureService { get; set; }

    public Core2(ISilmoonConfigureService silmoonConfigureService)
    {
        SilmoonConfigureService = (SilmoonConfigureServiceImpl)silmoonConfigureService;
        Executer = new MongoExecuter(SilmoonConfigureService.MongoDBConnectionString);
    }
    public User GetUser(string Username)
    {
        if (Username.IsNullOrEmpty()) return null;
        return new User()
        {
            Username = Username,
            Password = "123123".GetMD5Hash(),
        };
    }
    public StateSet<bool> NewUser(User user)
    {
        return true.ToStateSet(user.Username);
    }

    public void Dispose()
    {
        Executer = null;
    }

}
