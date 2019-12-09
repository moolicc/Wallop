using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WallApp.Bridge;
using WallApp.Bridge.Data;

namespace WallApp.Bridge
{
    public delegate void PayloadHandler(IPayload payload);

    public class MessageScheduler
    {
        private InputReader<IPayload> _reader;

        private Dictionary<Type, Queue<ConsumerEntry>> _consumers;
        private bool _consumerLock;

        private bool _consumingNext;
        private IPayload _consumeNext;

        public MessageScheduler(InputReader<IPayload> reader)
        {
            _reader = reader;

            _consumers = new Dictionary<Type, Queue<ConsumerEntry>>();

            Task.Run(Run);
        }

        public void RegisterMessage<T>(PayloadHandler action, int timeoutTime, Action timeoutAction) where T : IPayload
        {
            while (_consumerLock) ;
            _consumerLock = true;

            Type tType = typeof(T);

            Queue<ConsumerEntry> entryQueue;
            if(!_consumers.TryGetValue(tType, out entryQueue))
            {
                entryQueue = new Queue<ConsumerEntry>();
                _consumers.Add(tType, entryQueue);
            }
            entryQueue.Enqueue(new ConsumerEntry(action, timeoutTime, timeoutAction));


            _consumerLock = false;
        }

        public T ConsumeNext<T>() where T : IPayload
        {
            throw new NotImplementedException("This won't even work... Figure out what the plan was and make it happen, chief!");
            _consumingNext = true;
            while (_consumeNext == null) ;

            var returnVal = (T)_consumeNext;
            _consumingNext = false;
            _consumeNext = null;

            return returnVal;
        }

        private void Run()
        {
            while (true)
            {
                while (_reader.Queue.Count > 0)
                {
                    if (!_consumerLock)
                    {
                        _consumerLock = true;

                        foreach (var item in _consumers)
                        {
                            foreach (var consumer in item.Value)
                            {
                                if(consumer.Timeout <= 0)
                                {
                                    continue;
                                }
                                if(consumer.TimeoutTimer.ElapsedMilliseconds > consumer.Timeout)
                                {
                                    consumer.TimedOut = true;
                                    consumer.OnTimeout();
                                }
                            }
                        }

                        _consumerLock = false;
                    }


                    if (!_reader.Queue.TryDequeue(out var payload))
                    {
                        break;
                    }

                    if(!_consumerLock)
                    {
                        _consumerLock = true;
                        if(_consumers.TryGetValue(payload.GetType(), out var entry))
                        {
                            var consumer = entry.Dequeue();
                            while(entry.Count > 0)
                            {
                                if(consumer.TimedOut)
                                {
                                    consumer = entry.Dequeue();
                                }
                            }

                            PayloadHandler handler = entry.Dequeue().Handler;
                            if(handler != null)
                            {
                                Task.Run(() => handler(payload));
                                _consumerLock = false;
                                continue;
                            }
                        }
                        _consumerLock = false;
                    }
                }
                Thread.Sleep(500);
            }
        }

        private class ConsumerEntry
        {
            public PayloadHandler Handler;
            public int Timeout;
            public Action OnTimeout;
            public Stopwatch TimeoutTimer;
            public bool TimedOut;

            public ConsumerEntry(PayloadHandler action, int timeout, Action onTimeout)
            {
                Handler = action;
                Timeout = timeout;
                OnTimeout = onTimeout;
                TimeoutTimer = new Stopwatch();
                TimedOut = false;

                if (OnTimeout != null)
                {
                    TimeoutTimer.Start();
                }
            }
        }
    }
}
