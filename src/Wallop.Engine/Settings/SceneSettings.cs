using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Settings
{
    public class StoredBinding
    {
        public string TypeName { get; set; }
        public string PropertyName { get; set; }
        public string SettingName { get; set; }
    }

    public class StoredModule
    {
        public string ModuleId { get; set; }
        public Dictionary<string, string> Settings { get; set; }
        public List<StoredBinding> StoredBindings { get; set; } = new List<StoredBinding>();
        public string InstanceName { get; set; }
    }

    public class StoredLayout
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public List<StoredModule> ActorModules { get; set; }
    }

    public class SceneSettings : Cog.Settings
    {
        public List<StoredModule> DirectorModules { get; set; }
        public List<StoredLayout> Layouts { get; set; }

        public SceneSettings()
        {
            DirectorModules = new List<StoredModule>();
            Layouts = new List<StoredLayout>();
        }

    }
}
