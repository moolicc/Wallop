using Cog.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog
{
    public class ConfigurationOptions
    {
        [Obsolete("Not yet implemented.")]
        public bool PersistSettingsOnChange { get; set; }
        public bool ConfigureBindings { get; set; }
        public List<ISettingsSource> Sources { get; private set; }

        public bool FlattenTree { get; set; }

        public string HierarchyDelimiter { get; set; }


        public ConfigurationOptions()
        {
            ConfigureBindings = true;
            Sources = new List<ISettingsSource>();
            FlattenTree = false;
            HierarchyDelimiter = ".";
        }
    }
}
