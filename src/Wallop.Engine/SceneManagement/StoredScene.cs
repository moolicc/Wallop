using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.SceneManagement
{
    public class StoredBinding
    {
        public string TypeName { get; set; }
        public string PropertyName { get; set; }
        public string SettingName { get; set; }

        public StoredBinding(string typeName, string propertyName, string settingName)
        {
            TypeName = typeName;
            PropertyName = propertyName;
            SettingName = settingName;
        }
    }

    public class StoredModule
    {
        public string ModuleId { get; set; }
        public Dictionary<string, string> Settings { get; set; }
        public List<StoredBinding> StoredBindings { get; set; }
        public string InstanceName { get; set; }

        public StoredModule()
        {
            ModuleId = "";
            Settings = new Dictionary<string, string>();
            StoredBindings = new List<StoredBinding>();
            InstanceName = "";
        }
    }

    public class StoredLayout
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public List<StoredModule> ActorModules { get; set; }

        public StoredLayout()
        {
            Name = "";
            Active = false;
            ActorModules = new List<StoredModule>();
        }
    }

    public class StoredScene
    {
        public string Name { get; set; }

        public List<StoredModule> DirectorModules { get; set; }
        public List<StoredLayout> Layouts { get; set; }

        public StoredScene()
        {
            Name = "";
            DirectorModules = new List<StoredModule>();
            Layouts = new List<StoredLayout>();
        }

    }
}
