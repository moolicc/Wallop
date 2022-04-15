using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Messaging
{
    public delegate void MessageListener<T>((T payload, uint messageId) message, ref bool handled) where T : struct;

    public class Messenger
    {
        private Dictionary<Type, IMessageQueue> _queues;
        private ushort _nextMessageId;

        public Messenger()
        {
            _queues = new Dictionary<Type, IMessageQueue>();
            _nextMessageId = 1;
        }

        public void RegisterQueue<T>() where T : struct
        {
            var queue = new MessageQueue<T>();
            _queues.Add(typeof(T), queue);
        }

        public bool Take<T>(ref T payload, ref uint messageId) where T : struct
        {
            if(!_queues.TryGetValue(typeof(T), out var queue))
            {
                queue = new MessageQueue<T>();
                _queues.Add(typeof(T), queue);
            }

            if (queue is MessageQueue<T> msgQueue)
            {
                bool succeeded = msgQueue.TakeNext(out var result) == TakeResults.Succeeded;
                payload = result.payload;
                messageId = result.messageId;
                return succeeded;
            }
            return false;
        }

        public (T Payload, uint MessageId)[] Take<T>(int count) where T : struct
        {
            return Take<T>(ref count);
        }

        public (T Payload, uint MessageId)[] Take<T>(ref int count) where T : struct
        {
            const int MAX_FAILURES = 5;

            var buffer = new (T, uint)[count];
            if (!_queues.TryGetValue(typeof(T), out var queue))
            {
                queue = new MessageQueue<T>();
                _queues.Add(typeof(T), queue);
            }

            if (queue is MessageQueue<T> msgQueue)
            {
                int failures = 0;
                int attemptCount = count;
                count = 0;
                for (int i = 0; i < attemptCount; i++)
                {
                    var result = msgQueue.TakeNext(out buffer[i]);
                    if (result == TakeResults.OutOfElements)
                    {
                        break;
                    }
                    if (result == TakeResults.Failed)
                    {
                        failures++;
                        i--;
                    }
                    if (failures >= MAX_FAILURES)
                    {
                        break;
                    }
                    count++;
                }
            }

            return buffer;
        }

        public uint Put<T>(T message) where T : struct
        {
            if (!_queues.TryGetValue(typeof(T), out var queue))
            {
                queue = new MessageQueue<T>();
                _queues.Add(typeof(T), queue);
            }

            uint msgId;
            ushort high;
            unchecked
            {
                high = _nextMessageId++;
            }

            if (queue is MessageQueue<T> msgQueue)
            {
                msgId = msgQueue.Enqueue(message, high);
            }
            else
            {
                return 0;
            }

            return msgId;
        }

        public void Listen<T>(MessageListener<T> listener) where T : struct
        {
            if (!_queues.TryGetValue(typeof(T), out var queue))
            {
                queue = new MessageQueue<T>();
                _queues.Add(typeof(T), queue);
            }
            if (queue is MessageQueue<T> msgQueue)
            {
                msgQueue.MessageListener += listener;
            }
        }

        public void ClearState()
        {
            foreach (var queue in _queues)
            {
                queue.Value.ClearState();
            }
        }
    }
}
