using Newtonsoft.Json.Linq;

namespace Silmoon.AspNetCore.Interfaces
{
    public interface ISilmoonConfigureService
    {
        public JObject ConfigJson { get; }
    }
}
