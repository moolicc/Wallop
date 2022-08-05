using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Modules.SettingTypes
{
    public interface ISettingType
    {
        string Name { get; }

        string Serialize(object value, IEnumerable<KeyValuePair<string, string>>? args);
        bool TrySerialize(object value, out string? result, IEnumerable<KeyValuePair<string, string>>? args);
        bool TryDeserialize(string value, out object? result, IEnumerable<KeyValuePair<string, string>>? args);
    }
}
