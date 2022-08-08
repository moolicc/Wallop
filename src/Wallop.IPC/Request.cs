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
        public string Message { get; init; }
        public string OtherApplicationId { get; init; }

        private ISerializer _serializer;


        private Request(ISerializer serializer, int requestId, string otherApplicationId, string message, bool failed)
        {
            RequestId = requestId;
            RequestFailed = failed;
            Message = message;
            _serializer = serializer;
            OtherApplicationId = otherApplicationId;
        }

        public T As<T>()
        {
            return _serializer.Deserialize<T>(Message);
        }

        public object ToObject()
        {
            return _serializer.Deserialize(Message);
        }


        public static Request Success(ISerializer serializer, string otherAppId, int requestId, string message)
            => new Request(serializer, requestId, otherAppId, message, false);

        public static Request Failed(ISerializer serializer, string otherAppId, int requestId, string reason)
            => new Request(serializer, requestId, otherAppId, reason, true);



        public static implicit operator string(Request res)
            => res.Message;
    }
}
