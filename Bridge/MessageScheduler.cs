using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Wallop.Bridge.Data;

namespace Wallop.Bridge
{
    public delegate void PayloadHandler(IPayload payload);

    public class MessageScheduler
    {
        private InputReader<IPayload> _reader;
        private Dictionary<Type, Queue<ConsumerEntry>> _consumers;
        private bool _consumerLock;
        private Dictionary<Type, IPayload> _immediateConsumers;
        private bool _immediateLock;


        public MessageScheduler(InputReader<IPayload> reader)
        {
            _reader = reader;
            _consumers = new Dictionary<Type, Queue<ConsumerEntry>>();
            _consumerLock = false;
            _immediateConsumers = new Dictionary<Type, IPayload>();
            _immediateLock = false;

            Task.Run(Run);
        }

        public void RegisterMessage<T>(PayloadHandler action, int timeoutTime, Action timeoutAction) where T : IPayload
        {
            while (_consumerLock) ;
            _consumerLock = true;

            Type tType = typeof(T);

            Queue<ConsumerEntry> entryQueue;
            if (!_consumers.TryGetValue(tType, out entryQueue))
            {
                entryQueue = new Queue<ConsumerEntry>();
                _consumers.Add(tType, entryQueue);
            }
            entryQueue.Enqueue(new ConsumerEntry(action, timeoutTime, timeoutAction));


            _consumerLock = false;
        }

        public T ConsumeNext<T>() where T : IPayload
        {
            while (_immediateLock) ;
            Type type = typeof(T);
            _immediateLock = true;
            if (!_immediateConsumers.TryGetValue(type, out var value))
            {
                _immediateConsumers.Add(type, null);
            }
            else
            {
                //TODO: Warning, this is not thread-safe!
            }
            if (value is T tVal)
            {
                return tVal;
            }

            _immediateLock = false;
            IPayload payload = _immediateConsumers[type];
            while (payload == null)
            {
                Thread.Sleep(100);
                payload = _immediateConsumers[type];
            }
            _immediateLock = true;
            _immediateConsumers.Remove(type);
            _immediateLock = false;
            return (T)payload;
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
                                if (consumer.Timeout <= 0)
                                {
                                    continue;
                                }
                                if (consumer.TimeoutTimer.ElapsedMilliseconds > consumer.Timeout)
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

                    if (!_immediateLock && _immediateConsumers.ContainsKey(payload.GetType()))
                    {
                        _immediateLock = true;
                        _immediateConsumers[payload.GetType()] = payload;
                        _immediateLock = false;
                        continue;
                    }

                    if (!_consumerLock)
                    {
                        _consumerLock = true;
                        if (_consumers.TryGetValue(payload.GetType(), out var entry))
                        {
                            var consumer = entry.Dequeue();
                            while (entry.Count > 0)
                            {
                                if (consumer.TimedOut)
                                {
                                    consumer = entry.Dequeue();
                                }
                            }

                            PayloadHandler handler = entry.Dequeue().Handler;
                            if (handler != null)
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
