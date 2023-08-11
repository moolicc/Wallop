using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Modules.SettingTypes;

namespace Wallop.Shared.Scripting
{
    public static class ScriptEngineServices
    {
        private class CaseInsensitiveComparer : IEqualityComparer<string>
        {
            public bool Equals(string? x, string? y)
            {
                return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode([DisallowNull] string obj)
            {
                return obj.ToLower().GetHashCode();
            }
        }

        public static Dictionary<string, object> CastArgs(IEnumerable<KeyValuePair<string, string>> engineArgs, Dictionary<string, Type> typeMap)
        {
            typeMap = new Dictionary<string, Type>(typeMap, new CaseInsensitiveComparer());

            var results = new Dictionary<string, object>();
            foreach (var engineArg in engineArgs)
            {
                if (!typeMap.TryGetValue(engineArg.Key, out var targetType))
                {
                    results.Add(engineArg.Key, engineArg.Value);
                    continue;
                }

                if (targetType == typeof(bool) && string.IsNullOrEmpty(engineArg.Value))
                {
                    results.Add(engineArg.Key, true);
                    continue;
                }

                // TODO: This is a very bad temporary solution for this.
                string valueInsertion = $"\"{engineArg.Value}\"";
                if (targetType.IsNumericType() || targetType == typeof(bool))
                {
                    valueInsertion = engineArg.Value.ToString().ToLower();
                }

                string json = $"{{ \"value\": {valueInsertion} }}";
                try
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize(valueInsertion, targetType);
                    results.Add(engineArg.Key, result!);
                }
                catch
                {
                    results.Add(engineArg.Key, engineArg.Value);
                }
            }

            return results;
        }

        private static bool IsNumericType(this Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
