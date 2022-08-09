using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Wallop.IPC;

namespace Wallop.Shared.Messaging.Remoting
{
    public class RemoteMessenger : IMessenger
    {
        public IpcAgent AgentClient { get; init; }
        public string HostApplication { get; init; }


        public RemoteMessenger(IpcAgent agentClient, string hostApplication)
        {
            AgentClient = agentClient;
            HostApplication = hostApplication;
        }

        public uint Put(ValueType message, Type messageType)
        {
            var encoded = Json.Json.WriteMessage(message, messageType);
            var outgoing = new PushMessage(MessageDirection.Put, encoded, null, null);

            var result = AgentClient.SendRequestAsync(outgoing, null, HostApplication);

            while (!result.IsCompleted) ;

            if(!result.Result.RequestFailed)
            {
                var reply = result.Result.As<PullMessage>();
                return reply.MessageID;
            }

            return 0;
        }

        public uint Put(ValueType message, Type messageType, uint? preferredId)
        {
            var encoded = Json.Json.WriteMessage(message, messageType);
            var outgoing = new PushMessage(MessageDirection.Put, encoded, null, preferredId);

            var result = AgentClient.SendRequestAsync(outgoing, null, HostApplication);

            while (!result.IsCompleted) ;

            if (!result.Result.RequestFailed)
            {
                var reply = result.Result.As<PullMessage>();
                return reply.MessageID;
            }

            return 0;
        }

        public uint Put<T>(T message) where T : struct
        {
            return Put(message, typeof(T));
        }

        public uint Put<T>(T message, uint? preferredId) where T : struct
        {
            if(!preferredId.HasValue)
            {
                return Put(message);
            }

            var encoded = Json.Json.WriteMessage(message, typeof(T));
            var outgoing = new PushMessage(MessageDirection.Put, encoded, null, preferredId);
            var result = AgentClient.SendRequestAsync(outgoing, null, HostApplication);

            while (!result.IsCompleted) ;

            if (!result.Result.RequestFailed)
            {
                var reply = result.Result.As<PullMessage>();
                return reply.MessageID;
            }

            return 0;
        }



        public bool Take(out ValueType? payload, Type targetType, ref uint messageId)
        {
            payload = null;

            var outgoing = new PushMessage(MessageDirection.Take, null, targetType.FullName, null);
            var data = JsonSerializer.Serialize(outgoing);

            var result = AgentClient.SendRequestAsync(outgoing, null, HostApplication);

            while (!result.IsCompleted) ;

            if (result.Result.RequestFailed)
            {
                return false;
            }

            var reply = result.Result.As<PullMessage>();
            if(reply.EncodedMessage == null)
            {
                return false;
            }

            messageId = reply.MessageID;
            var response = JsonSerializer.Deserialize(reply.EncodedMessage.MessageData, targetType);
            payload = (ValueType)response!;
            return true;
        }

        public bool Take<T>(ref T payload, ref uint messageId) where T : struct
        {
            var targetType = typeof(T);
            var outgoing = new PushMessage(MessageDirection.Take, null, targetType.FullName, null);
            var data = JsonSerializer.Serialize(outgoing);

            var result = AgentClient.SendRequestAsync(outgoing, null, HostApplication);

            while (!result.IsCompleted) ;

            if (result.Result.RequestFailed)
            {
                return false;
            }

            var reply = result.Result.As<PullMessage>();
            if (reply.EncodedMessage == null)
            {
                return false;
            }

            messageId = reply.MessageID;
            var response = JsonSerializer.Deserialize(reply.EncodedMessage.MessageData, targetType);
            payload = (T)response!;
            return true;
        }

        public MessageProxy<T>[] Take<T>(int count) where T : struct
        {
            throw new NotImplementedException();
        }

        public MessageProxy<T>[] Take<T>(ref int count) where T : struct
        {
            throw new NotImplementedException();
        }

        public void RegisterQueue<T>() where T : struct
        {
            throw new NotImplementedException();
        }

        public void RemovePutHook(MessageHook hook)
        {
            throw new NotImplementedException();
        }

        public void RemoveTakeHook(MessageHook hook)
        {
            throw new NotImplementedException();
        }

        public void AddPutHook(MessageHook hook)
        {
            throw new NotImplementedException();
        }

        public void AddTakeHook(MessageHook hook)
        {
            throw new NotImplementedException();
        }

        public void Listen<T>(MessageListener<T> listener) where T : struct
        {
            throw new NotImplementedException();
        }

        public void ClearState()
        {
            throw new NotImplementedException();
        }
    }
}
