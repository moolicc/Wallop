using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Modules.SettingTypes
{
    public enum RealNumberPrecision
    {
        Single,
        Double,
        Decimal,
    }

    public class RealNumberType : ISettingType
    {
        public string Name => "real";

        public string Serialize(object value, IEnumerable<KeyValuePair<string, string>>? args)
        {
            return value.ToString() ?? "0.0";
        }

        public bool TrySerialize(object value, [NotNullWhen(true)] out string? result, IEnumerable<KeyValuePair<string, string>>? args)
        {
            result = value.ToString();
            return result != null;
        }

        public bool TryDeserialize(string value, [NotNullWhen(true)] out object? result, IEnumerable<KeyValuePair<string, string>>? args)
        {
            var precision = RealNumberPrecision.Single;
            if(args != null)
            {
                foreach (var arg in args)
                {
                    if (arg.Key.Equals("precision", StringComparison.OrdinalIgnoreCase))
                    {
                        Enum.TryParse(arg.Value, out precision);
                    }
                }
            }
            bool successful = false;
            if (precision == RealNumberPrecision.Single)
            {
                successful = float.TryParse(value, out var parsed);
                result = parsed;
            }
            else if (precision == RealNumberPrecision.Double)
            {
                successful = double.TryParse(value, out var parsed);
                result = parsed;
            }
            else
            {
                successful = decimal.TryParse(value, out var parsed);
                result = parsed;
            }
            return successful;
        }
    }
}
