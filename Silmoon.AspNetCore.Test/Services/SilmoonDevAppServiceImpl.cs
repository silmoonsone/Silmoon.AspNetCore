using Microsoft.Extensions.Options;
using Silmoon.AspNetCore.Services;
using Silmoon.Models;
using System.Runtime.CompilerServices;

namespace Silmoon.AspNetCore.Test.Services
{
    public class SilmoonDevAppServiceImpl : SilmoonDevAppService
    {
        public Core Core { get; set; }

        public SilmoonDevAppServiceImpl(Core core, IOptions<SilmoonDevAppServiceOptions> options) : base(options)
        {
            Core = core;
        }

        public override Task<StateSet<bool, (string SignatureKey, string EncryptKey)>> GetKey(string AppId)
        {
            return Task.FromResult(StateSet<bool, (string, string)>.Create(false, (default, default), "No such AppId"));
        }
    }
}