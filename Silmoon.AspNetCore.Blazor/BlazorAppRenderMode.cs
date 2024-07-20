using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Blazor
{
    public class BlazorAppRenderMode
    {
        public static InteractiveServerRenderMode ServerPrerendered = new(true);
        public static InteractiveServerRenderMode Server = new(false);
    }
}
