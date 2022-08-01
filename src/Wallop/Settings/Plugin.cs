using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Settings
{
    public class Plugin
    {
        public string PluginDll { get; set; } = string.Empty;
        public bool PluginEnabled { get; set; }
    }
}
