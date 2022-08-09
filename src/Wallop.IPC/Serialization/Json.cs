using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Wallop.IPC.Serialization
{
    public class Json : ISerializer
    {
        public T Deserialize<T>(string data)
        {
            //var jObject = JsonSerializer.Deserialize<JsonData>(data);
            //var type = Type.GetType(jObject.Type)!;

            //if (type.IsAssignableTo(typeof(IConvertible)))
            //{
            //    return (T)Convert.ChangeType(jObject.Data, type);
            //}

            //return (T)JsonSerializer.Deserialize(jObject.Data, type)!;


            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new IntermediateConverter()
                }
            };
            return JsonSerializer.Deserialize<T>(data, options)!;
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

            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new IntermediateConverter()
                }
            };
            return JsonSerializer.Serialize(data, data.GetType(), options);
        }

        private class IntermediateConverter : JsonConverter<IntermediateValue>
        {
            public override IntermediateValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                // Read Type discriminator
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propName = reader.GetString();
                if (propName != nameof(IntermediateValue.ValueType))
                {
                    throw new JsonException();
                }

                reader.Read();
                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException();
                }

                var typeString = reader.GetString()!;

                // Read serialized value
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                propName = reader.GetString();
                if (propName != nameof(IntermediateValue.SerializedValue))
                {
                    throw new JsonException();
                }

                reader.Read();
                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException();
                }

                var serialized = reader.GetString()!;
                var type = TypeHelper.GetTypeByName(typeString)!;
                var value = JsonSerializer.Deserialize(serialized, type)!;

                reader.Read();
                if (reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException();
                }

                return new IntermediateValue(serialized, value);
            }

            public override void Write(Utf8JsonWriter writer, IntermediateValue value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                writer.WriteString(nameof(IntermediateValue.ValueType), value.ValueType.FullName!);
                writer.WriteString(nameof(IntermediateValue.SerializedValue), value.SerializedValue);

                writer.WriteEndObject();
            }
        }
    }
}
