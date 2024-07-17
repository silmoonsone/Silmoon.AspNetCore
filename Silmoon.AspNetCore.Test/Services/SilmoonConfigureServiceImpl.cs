using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Services;

namespace Silmoon.AspNetCore.Test.Services
{
    public class SilmoonConfigureServiceImpl : SilmoonConfigureService
    {
        public string MongoDBConnectionString { get; set; }
        public SilmoonConfigureServiceImpl(IOptions<SilmoonConfigureServiceOption> options) : base(options)
        {
            MongoDBConnectionString = ConfigJson["mongodb"].Value<string>();
        }
    }
}
