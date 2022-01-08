using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry.Extending
{
    public record PluginInformation()
    {
        public Exposed Exposed { get; internal set; }
        public Type PluginType { get; internal set; }
        public object? PluginObject { get; internal set; }
        public string PluginId { get; internal set; }
    }

    public interface IEntryPointContext
    {
        public PluginInformation PluginInformation { get; internal set; }
    }
}
