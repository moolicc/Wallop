using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Modules.SettingTypes
{
    public class TypeCache
    {
        public Dictionary<string, ISettingType> Types { get; private set; }

        public TypeCache()
        {
            Types = new Dictionary<string, ISettingType>();

            Add(new RealNumberType());
        }

        public T GetType<T>(string name) where T : ISettingType
        {
            return (T)Types[name];
        }

        public void Add<T>(T instance) where T : ISettingType
        {
            Types.Add(instance.Name, instance);
        }


        public string Serialize(string type, object value, IEnumerable<KeyValuePair<string, string>>? args)
        {
            return Types[type].Serialize(value, args);
        }

        public bool TryDeserialize(string type, string value, out object? result, IEnumerable<KeyValuePair<string, string>>? args)
        {
            if(!Types.TryGetValue(type, out var instance))
            {
                result = "";
                return false;
            }

            bool successful = instance.TryDeserialize(value, out var parsed, args);
            result = parsed;
            return successful;
        }
    }
}
