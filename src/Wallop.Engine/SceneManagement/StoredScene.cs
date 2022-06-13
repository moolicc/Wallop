using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.SceneManagement
{
    static class Extensions
    {
        public static StoredSetting Add(this List<StoredSetting> source, string name, string value, Type? trackedType = null)
        {
            var item = new StoredSetting(name, value, trackedType);
            source.Add(item);
            return item;
        }

        public static bool ContainsSetting(this List<StoredSetting> source, string name)
        {
            return source.Any(i => i.Name == name);
        }
    }

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
        public List<StoredSetting> Settings { get; set; }
        public List<StoredBinding> StoredBindings { get; set; }
        public string InstanceName { get; set; }

        public StoredModule()
        {
            ModuleId = "";
            Settings = new List<StoredSetting>();
            StoredBindings = new List<StoredBinding>();
            InstanceName = "";
        }
    }

    public class StoredSetting
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsTracked { get; set; }
        public Type? TrackedType { get; set; }

        public StoredSetting(string name, string value, Type? trackedType = null)
        {
            Name = name;
            Value = value;
            IsTracked = trackedType != null;
            TrackedType = trackedType;
        }

        public static IEnumerable<StoredSetting> FromEnumerable(IEnumerable<KeyValuePair<string, string>> enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return new StoredSetting(item.Key, item.Value);
            }
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
        public string? ConfigFile { get; set; }

        public StoredScene()
        {
            Name = "";
            DirectorModules = new List<StoredModule>();
            Layouts = new List<StoredLayout>();
            ConfigFile = null;
        }

    }
}
