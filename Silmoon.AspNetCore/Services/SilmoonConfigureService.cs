using System;
using System.IO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Silmoon.AspNetCore.Interfaces;
using Silmoon.Extension;

namespace Silmoon.AspNetCore.Services
{
    public class SilmoonConfigureService : ISilmoonConfigureService
    {
        public JObject ConfigJson { get; private set; }
        private SilmoonConfigureServiceOption Options { get; set; }
        public string CurrentConfigFilePath { get; set; }

        public SilmoonConfigureService(IOptions<SilmoonConfigureServiceOption> options)
        {
            Options = options.Value;
            if (Options.IsDebug is null)
            {
                //抛出异常，必须指定是否是Debug模式，使用SilmoonConfigureServiceOption.DebugConfig()或SilmoonConfigureServiceOption.ReleaseConfig()方法指定。
                throw new Exception("IsDebug must be specified, use SilmoonConfigureServiceOption.DebugConfig() or SilmoonConfigureServiceOption.ReleaseConfig() method to specify.");
            }
            else
            {
                if (File.Exists(Options.LocalConfigFile))
                    CurrentConfigFilePath = Options.LocalConfigFile;
                else
                    CurrentConfigFilePath = Options.DefaultConfigFile;


                ConfigJson = JsonHelperV2.LoadJsonFromFile(CurrentConfigFilePath);
            }
        }
    }
}
