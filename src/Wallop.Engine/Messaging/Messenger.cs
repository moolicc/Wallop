using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Messaging
{
    public delegate void MessageListener<T>(T message, ref bool handled) where T : struct;

    public class Messenger
    {
        private Dictionary<Type, IMessageQueue> _queues;

        public Messenger()
        {
            _queues = new Dictionary<Type, IMessageQueue>();
        }

        public void RegisterQueue<T>() where T : struct
        {
            var queue = new MessageQueue<T>();
            _queues.Add(typeof(T), queue);
        }

        public bool Take<T>(ref T result) where T : struct
        {
            var queue = _queues[typeof(T)];

            if (queue is MessageQueue<T> msgQueue)
            {
                return msgQueue.TakeNext(out result) == TakeResults.Succeeded;
            }
            return false;
        }

        public T[] Take<T>(int count) where T : struct
        {
            const int MAX_FAILURES = 5;

            T[] buffer = new T[count];
            var queue = _queues[typeof(T)];

            if(queue is MessageQueue<T> msgQueue)
            {
                int failures = 0;
                for (int i = 0; i < count; i++)
                {
                    var result = msgQueue.TakeNext(out buffer[i]);
                    if (result == TakeResults.OutOfElements)
                    {
                        break;
                    }
                    if(result == TakeResults.Failed)
                    {
                        failures++;
                        i--;
                    }
                    if(failures >= MAX_FAILURES)
                    {
                        break;
                    }
                }
            }

            return buffer;
        }

        public void Put<T>(T message) where T : struct
        {
            var queue = _queues[typeof(T)];
            if (queue is MessageQueue<T> msgQueue)
            {
                msgQueue.Enqueue(message);
            }
        }

        public void Listen<T>(MessageListener<T> listener) where T : struct
        {
            var queue = _queues[typeof(T)];
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
