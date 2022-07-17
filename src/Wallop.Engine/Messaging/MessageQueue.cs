using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Messaging
{
    public enum TakeResults
    {
        Succeeded = 1,
        OutOfElements,
        Failed
    }

    internal interface IMessageQueue
    {
        void ClearState();
    }

    public class MessageQueue<T> : IMessageQueue where T : struct
    {
        public event MessageListener<T>? MessageListener;

        public bool IsEmpty => _queue.IsEmpty;

        private ConcurrentQueue<(T payload, uint msgId)> _queue;
        private ushort _nextId;

        public MessageQueue()
        {
            _queue = new ConcurrentQueue<(T, uint)>();
            _nextId = 1;
        }


        public TakeResults TakeNext(out (T payload, uint messageId) result)
        {
            result = (default(T), 0);

            if (_queue.IsEmpty)
            {
                return TakeResults.OutOfElements;
            }
            else if(!_queue.TryDequeue(out result))
            {
                return TakeResults.Failed;
            }

            
            return TakeResults.Succeeded;
        }

        public uint Enqueue(T value, ushort highId)
        {
            ushort low = 0;
            unchecked
            {
                low = _nextId++;
            }
            uint messageId = (uint)highId << 16 | low;

            return Enqueue(value, messageId);
        }

        public uint Enqueue(T value, uint messageId)
        {
            bool handled = false;

            if (MessageListener != null)
            {
                MessageListener((value, messageId), ref handled);
            }

            if (!handled)
            {
                _queue.Enqueue((value, messageId));
            }
            return messageId;
        }

        public void ClearState()
        {
            MessageListener = null;
        }
    }
}
