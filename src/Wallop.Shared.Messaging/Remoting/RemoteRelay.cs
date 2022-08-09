using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.IPC;
using System.Text.Json;
using Wallop.IPC.Serialization;

namespace Wallop.Shared.Messaging.Remoting
{

    public class MessageRelay
    {
        public IpcAgent HostAgent { get; init; }
        public IMessenger Messenger { get; init; }

        public MessageRelay(IpcAgent hostAgent, IMessenger messenger)
        {
            HostAgent = hostAgent;
            Messenger = messenger;

            hostAgent.RequestReceivedCallback = OnRequest;
        }

        private object? OnRequest(Request request)
        {
            var push = request.As<PushMessage>();
            object? outgoing = new PullMessage(0, null);
            if (push.Direction == MessageDirection.Put && push.PutMessage != null)
            {
                var messageType = TypeHelper.GetTypeByName(push.PutMessage.MessageType)!;
                var message = JsonSerializer.Deserialize(push.PutMessage.MessageData, messageType)!;

                var id = Messenger.Put((ValueType)message, messageType, push.PreferredId);

                outgoing = new PullMessage(id, null);
            }
            else if (push.Direction == MessageDirection.Take && push.TakeType != null)
            {
                uint id = 0;

                var type = TypeHelper.GetTypeByName(push.TakeType);
                if (Messenger.Take(out var payload, type, ref id))
                {
                    var jMessage = Json.Json.WriteMessage(payload!, type);

                    outgoing = new PullMessage(id, jMessage);
                    return outgoing;
                }
            }
            return outgoing;
        }
    }
    


    /*
    internal enum MessageDirection
    {
        Put,
        Take,
    }

    internal readonly record struct MessengerMessage(MessageDirection Direction, uint MessageID, Json.JsonMessage EncodedMessage);

    public class MessageRelay
    {
        public string RelayApplication { get; set; }
        private IpcNode _node;
        private IMessenger _messenger;


        public MessageRelay(IIpcEndpoint endPoint, IMessenger messenger, string relayApplication = "*")
        {
            _node = new IpcNode(endPoint);
            _node.OnDataReceived2 = OnDataReceived;

            _messenger = messenger;
            _messenger.AddPutHook(OnPut);
            _messenger.AddTakeHook(OnTake);

            RelayApplication = relayApplication;
        }

        private void OnPut(uint[] messageIds, ValueType[] messages, Type messageType)
        {
            var jMessage = Json.Json.WriteMessage(messages[0], messageType);
            var outgoing = new MessengerMessage(MessageDirection.Put, messageIds[0], jMessage);
            var json = JsonSerializer.Serialize(outgoing);

            _node.Send(json, RelayApplication);
        }

        private void OnTake(uint[] messageIds, ValueType[] messages, Type messageType)
        {
            for (int i = 0; i < messageIds.Length; i++)
            {
                var jMessage = Json.Json.WriteMessage(messages[i], messageType);
                var outgoing = new MessengerMessage(MessageDirection.Take, messageIds[i], jMessage);
                var json = JsonSerializer.Serialize(outgoing);

                _node.Send(json, RelayApplication);
            }
        }

        private void OnDataReceived(IpcNode sender, IpcMessage message)
        {
            var incoming = JsonSerializer.Deserialize<MessengerMessage>(message.Content);

            var appMessageType = Type.GetType(incoming.EncodedMessage.MessageType)!;
            var appMessage = JsonSerializer.Deserialize(incoming.EncodedMessage.MessageData, appMessageType)!;

            if(incoming.Direction == MessageDirection.Put)
            {
                _messenger.Put((ValueType)appMessage, appMessageType);
            }
            else if(incoming.Direction == MessageDirection.Take)
            {
                ValueType payload = (ValueType)appMessage;
                uint messageId = 0;
                _messenger.Take(ref payload, appMessageType, ref messageId);
            }
        }
    }
    */
}
