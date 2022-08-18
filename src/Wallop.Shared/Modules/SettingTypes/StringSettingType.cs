using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Modules.SettingTypes
{
    public class StringSettingType : ISettingType
    {
        public string Name => "string";

        public string Serialize(object value, IEnumerable<KeyValuePair<string, string>>? args)
        {
            return value.ToString() ?? "";
        }

        public bool TryDeserialize(string value, [NotNullWhen(true)] out object? result, IEnumerable<KeyValuePair<string, string>>? args)
        {
            result = value;
            return true;
        }

        public bool TrySerialize(object value, [NotNullWhen(true)] out string? result, IEnumerable<KeyValuePair<string, string>>? args)
        {
            result = value.ToString();
            return result != null;
        }
    }
}
