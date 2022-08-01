using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Settings
{
    public class PluginSettings : Cog.Settings
    {
        public Plugin[] Plugins { get; set; } = Array.Empty<Plugin>();
    }
}
