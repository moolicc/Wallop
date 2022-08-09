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
            //var jObject = JsonSerializer.Deserialize<JsonData>(data);
            //var type = Type.GetType(jObject.Type)!;

            //if (type.IsAssignableTo(typeof(IConvertible)))
            //{
            //    return (T)Convert.ChangeType(jObject.Data, type);
            //}

            //return (T)JsonSerializer.Deserialize(jObject.Data, type)!;


            return JsonSerializer.Deserialize<T>(data)!;
        }

        public string Serialize(object data)
        {
            //var text = data.ToString();
            //var type = data.GetType();
            //if(type.IsAssignableTo(typeof(IConvertible)))
            //{
            //    text = Convert.ChangeType(data, typeof(string)).ToString();
            //}
            //else
            //{
            //    text = JsonSerializer.Serialize(data);
            //}
            //var jObject = new JsonData(data.GetType().FullName, text!);
            //return JsonSerializer.Serialize(jObject);

            return JsonSerializer.Serialize(data, data.GetType());
        }

        public object Deserialize(string data)
        {
            //var jObject = JsonSerializer.Deserialize<JsonData>(data);
            //var type = Type.GetType(jObject.Type)!;

            //if(type.IsAssignableTo(typeof(IConvertible)))
            //{
            //    return Convert.ChangeType(jObject.Data, type);
            //}

            //return JsonSerializer.Deserialize(jObject.Data, type)!;

            return JsonSerializer.Deserialize(data, typeof(object))!;
        }

        public T Deserialize<T>(object intermediateData)
        {
            if(intermediateData is JsonElement jNode)
            {
                return jNode.Deserialize<T>()!;
            }
            var type = intermediateData.GetType();
            throw new InvalidOperationException("Invalid intermediate data.");
        }

        public object Deserialize(object intermediateData, Type targetType)
        {
            if (intermediateData.GetType().IsAssignableFrom(typeof(JsonElement)))
            {
                return ((JsonNode)intermediateData).Deserialize(targetType)!;
            }
            throw new InvalidOperationException("Invalid intermediate data.");
        }
    }
}
