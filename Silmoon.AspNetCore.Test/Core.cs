using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Extension.CoreHelpers;
using Silmoon.AspNetCore.Services.Interfaces;
using Silmoon.AspNetCore.Test.Models;
using Silmoon.AspNetCore.Test.Services;
using Silmoon.Data.MongoDB;
using Silmoon.Extension;
using Silmoon.Models;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Silmoon.AspNetCore.Test;

public class Core : MongoService, IDisposable
{
    public override MongoExecuter Executer { get; set; }
    public ISilmoonConfigureService SilmoonConfigureService { get; set; }

    public Core(ISilmoonConfigureService silmoonConfigureService)
    {
        SilmoonConfigureService = silmoonConfigureService;
        Executer = new MongoExecuter(((SilmoonConfigureServiceImpl)SilmoonConfigureService).MongoDBConnectionString);
    }
    public User GetUser(string Username)
    {
        if (Username.IsNullOrEmpty()) return null;
        return new User()
        {
            Username = Username,
            Password = "123",
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
