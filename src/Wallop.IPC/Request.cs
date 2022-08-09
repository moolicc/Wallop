using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.IPC.Serialization;

namespace Wallop.IPC
{
    public class Request
    {
        public int RequestId { get; init; }
        public bool RequestFailed { get; init; }
        public object SerializedMessage { get; init; }
        public string OtherApplicationId { get; init; }

        private ISerializer _serializer;


        private Request(ISerializer serializer, int requestId, string otherApplicationId, object serializedMessage, bool failed)
        {
            RequestId = requestId;
            RequestFailed = failed;
            SerializedMessage = serializedMessage;
            _serializer = serializer;
            OtherApplicationId = otherApplicationId;
        }

        public T As<T>()
        {
            //return (T)Message;
            return _serializer.Deserialize<T>(SerializedMessage);
        }

        public object ToObject(Type type)
        {
            return _serializer.Deserialize(SerializedMessage, type);
        }


        public static Request Success(ISerializer serializer, string otherAppId, int requestId, string message)
            => new Request(serializer, requestId, otherAppId, message, false);

        public static Request Success(ISerializer serializer, string otherAppId, int requestId, object message)
            => new Request(serializer, requestId, otherAppId, message, false);


        public static Request Failed(ISerializer serializer, string otherAppId, int requestId, string reason)
            => new Request(serializer, requestId, otherAppId, reason, true);



        //public static implicit operator string(Request res)
        //    => res.Message;
    }
}
