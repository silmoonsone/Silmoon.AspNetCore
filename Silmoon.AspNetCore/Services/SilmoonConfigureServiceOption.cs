using System.Reflection;

namespace Silmoon.AspNetCore.Services
{
    public class SilmoonConfigureServiceOption
    {
#if DEBUG
        public string DefaultConfigFile { get; private set; } = "config.debug.json";
        public string ConfigFile { get; private set; } = "config-local.debug.json";
        public bool? IsDebug { get; private set; }
#else
        public string DefaultConfigFile { get; set; } = "config.json";
        public string ConfigFile { get; set; } = "config-local.json";
        public bool? IsDebug { get; private set; } = false;
#endif
        public SilmoonConfigureServiceOption()
        {

        }
        public void DebugConfig(string DefaultConfigFile = "config.debug.json", string ConfigFile = "config-local.debug.json")
        {
            IsDebug = true;
            this.DefaultConfigFile = DefaultConfigFile;
            this.ConfigFile = ConfigFile;
        }
        public void ReleaseConfig(string DefaultConfigFile = "config.json", string ConfigFile = "config-local.json")
        {
            IsDebug = false;
            this.DefaultConfigFile = DefaultConfigFile;
            this.ConfigFile = ConfigFile;
        }
    }
}
