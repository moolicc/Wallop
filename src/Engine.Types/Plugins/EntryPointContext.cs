using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Types.Plugins
{
    public abstract class EntryPointContext : PluginPantry.Extending.EntryPointContext
    {
        public abstract void LoadSettings<T>();
        public abstract void SaveSettings<T>();

    }
}
