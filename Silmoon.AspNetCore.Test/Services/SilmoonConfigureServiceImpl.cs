using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Services;

namespace Silmoon.AspNetCore.Test.Services
{
    public class SilmoonConfigureServiceImpl : SilmoonConfigureService
    {
        public string MongoDBConnectionString { get; private set; }
        public ILogger<SilmoonConfigureService> Logger { get; private set; }
        public SilmoonConfigureServiceImpl(IOptions<SilmoonConfigureServiceOption> options, ILogger<SilmoonConfigureService> logger) : base(options)
        {
            MongoDBConnectionString = ConfigJson["mongodb"].Value<string>();
            Logger = logger;
            Logger.LogInformation($"Config file is {CurrentConfigFilePath}.");
        }
    }
}
