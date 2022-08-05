using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging.Json
{
    public static class Json
    {

        static Json()
        {
        }


        public static JsonMessage WriteMessage(ValueType message, Type messageType)
        {
            var type = messageType.FullName!;
            var jobject = JsonSerializer.SerializeToNode(message, messageType)!;
            

            return new JsonMessage(type, (JsonObject)jobject);
        }

        public static IEnumerable<(ValueType Value, Type MessageType)> ParseMessages(string jsonSource)
        {
            var root = JsonSerializer.Deserialize<JsonMessages>(jsonSource);

            if (root == null)
            {
                throw new InvalidOperationException("Failed to parse json source.");
            }

            foreach (var item in root.Messages)
            {
                if (item != null)
                {
                    var type = Type.GetType(item.MessageType)!;
                    var result = JsonSerializer.Deserialize(item.MessageData, type);

                    if (result == null)
                    {
                        throw new InvalidOperationException("Failed to parse message from json.");
                    }
                    yield return ((ValueType)result, type);
                }
            }
        }
    }
}
