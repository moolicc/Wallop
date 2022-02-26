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

        private ConcurrentQueue<T> _queue;

        public MessageQueue()
        {
            _queue = new ConcurrentQueue<T>();
        }


        public TakeResults TakeNext(out T result)
        {
            result = default(T);

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

        public void Enqueue(T value)
        {
            bool handled = false;
            if(MessageListener != null)
            {
                MessageListener(value, ref handled);
            }
            if(!handled)
            {
                _queue.Enqueue(value);
            }
        }

        public void ClearState()
        {
            MessageListener = null;
        }
    }
}
