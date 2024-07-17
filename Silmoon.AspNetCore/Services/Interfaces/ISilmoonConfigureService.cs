using Newtonsoft.Json.Linq;

namespace Silmoon.AspNetCore.Services.Interfaces
{
    public interface ISilmoonConfigureService
    {
        public JObject ConfigJson { get; }
    }
}
