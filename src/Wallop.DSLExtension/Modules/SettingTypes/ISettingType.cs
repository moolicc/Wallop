using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Modules.SettingTypes
{
    public interface ISettingType
    {
        public string Name { get; }

        public string Serialize(object value, IEnumerable<KeyValuePair<string, string>>? args);
        public bool TryDeserialize(string value, out object? result, IEnumerable<KeyValuePair<string, string>>? args);
    }
}
