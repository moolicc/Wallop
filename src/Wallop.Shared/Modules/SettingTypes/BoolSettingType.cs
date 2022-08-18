using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Modules.SettingTypes
{
    public class BoolSettingType : ISettingType
    {
        public string Name => "boolean";

        public string Serialize(object value, IEnumerable<KeyValuePair<string, string>>? args)
        {
            if(value is bool b)
            {
                return b.ToString();
            }
            throw new InvalidOperationException("Invalid type.");
        }

        public bool TryDeserialize(string value, [NotNullWhen(true)] out object? result, IEnumerable<KeyValuePair<string, string>>? args)
        {
            if(value.Equals("1") || value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                return true;
            }
            else if(value.Equals("0") || value.Equals("false", StringComparison.OrdinalIgnoreCase) || value.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                result = false;
                return true;
            }

            result = null;
            return false;
        }

        public bool TrySerialize(object value, [NotNullWhen(true)] out string? result, IEnumerable<KeyValuePair<string, string>>? args)
        {
            result = null;
            if (value is bool b)
            {
                result = b.ToString();
                return true;
            }
            return false;
        }
    }
}
