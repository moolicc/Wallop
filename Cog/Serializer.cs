using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;

// TODO: Replace this with a better/extensible solution.

namespace Cog
{
    public static class Serializer
    {
        public static object? Deserialize(Type targetType, string value)
        {

            if(targetType.IsEnum)
            {
                if(Enum.GetNames(targetType).Any())
                {
                    var underlyingType = Enum.GetUnderlyingType(targetType);
                    var eValue = Enum.Parse(targetType, value);
                    value = Convert.ChangeType(eValue, underlyingType).ToString();
                }
            }

            var result = JsonSerializer.Deserialize(value, targetType);
            return result;
        }

        public static string Serialize(Type valueType, object? value)
        {
            var result = JsonSerializer.Serialize(value, valueType);
            return result;
        }
    }
}
