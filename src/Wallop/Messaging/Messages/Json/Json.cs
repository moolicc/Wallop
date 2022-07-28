using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Messaging.Messages.Json
{
    public static class Json
    {
        private static Dictionary<MessageTypes, Type> _typeMap;

        static Json()
        {
            _typeMap = new Dictionary<MessageTypes, Type>
            {
                { MessageTypes.ActiveLayout, typeof(SetActiveLayoutMessage) },
                { MessageTypes.AddActor, typeof(AddActorMessage) },
                { MessageTypes.AddDirector, typeof(AddDirectorMessage) },
                { MessageTypes.AddLayout, typeof(AddLayoutMessage) },
                { MessageTypes.ChangeScene, typeof(SceneChangeMessage) },
                { MessageTypes.CreateScene, typeof(CreateSceneMessage) },
                { MessageTypes.GraphicsChange, typeof(GraphicsMessage) },
                { MessageTypes.ReloadModule, typeof(ReloadModuleMessage) },
                { MessageTypes.SaveScene, typeof(SceneSaveMessage) },
                { MessageTypes.SceneSettingsChange, typeof(SceneSettingsMessage) },

            };
        }

        public static Type GetType(MessageTypes type)
        {
            return _typeMap[type];
        }

        public static MessageTypes GetType(Type type)
        {
            return _typeMap.First(kvp => kvp.Value == type).Key;
        }

        
        public static IEnumerable<(object Value, Type MessageType)> ParseMessages(string jsonSource)
        {
            var root = System.Text.Json.JsonSerializer.Deserialize<JsonMessages>(jsonSource);

            if(root == null)
            {
                throw new InvalidOperationException("Failed to parse json source.");
            }

            foreach (var item in root.Messages)
            {
                if(item != null)
                {
                    var type = GetType(item.MessageType);
                    var result = System.Text.Json.JsonSerializer.Deserialize(item.MessageData, type);

                    if(result == null)
                    {
                        throw new InvalidOperationException("Failed to parse message from json.");
                    }
                    yield return (result, type);
                }
            }
        }
    }
}
