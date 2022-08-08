using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Wallop.IPC.Serialization
{
    public class Json : ISerializer
    {
        private readonly record struct JsonData(string Type, string Data);

        public T Deserialize<T>(string data)
        {
            var jObject = JsonSerializer.Deserialize<JsonData>(data);
            var type = Type.GetType(jObject.Type)!;

            if (type.IsAssignableTo(typeof(IConvertible)))
            {
                return (T)Convert.ChangeType(jObject.Data, type);
            }

            return (T)JsonSerializer.Deserialize(jObject.Data, type)!;
        }

        public string Serialize(object data)
        {
            var text = data.ToString();
            var type = data.GetType();
            if(type.IsAssignableTo(typeof(IConvertible)))
            {
                text = Convert.ChangeType(data, typeof(string)).ToString();
            }
            else
            {
                text = JsonSerializer.Serialize(data);
            }
            var jObject = new JsonData(data.GetType().FullName, text!);
            return JsonSerializer.Serialize(jObject);
        }

        public object Deserialize(string data)
        {
            var jObject = JsonSerializer.Deserialize<JsonData>(data);
            var type = Type.GetType(jObject.Type)!;

            if(type.IsAssignableTo(typeof(IConvertible)))
            {
                return Convert.ChangeType(jObject.Data, type);
            }

            return JsonSerializer.Deserialize(jObject.Data, type)!;
        }
    }
}
