using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC.Serialization
{
    public class IntermediateValue
    {
        public object Value { get; set; }
        public string SerializedValue { get; set; }
        public Type ValueType { get; set; }


        public IntermediateValue(string serializedValue, object value)
        {
            SerializedValue = serializedValue;
            ValueType = value.GetType();
            Value = value;
        }

        public static IntermediateValue FromSerialized(ISerializer serializer, object value)
        {
            var serialized = serializer.Serialize(value);
            return new IntermediateValue(serialized, value);
        }
    }
}
