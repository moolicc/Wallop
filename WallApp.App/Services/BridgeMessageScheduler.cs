using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WallApp.Bridge;
using WallApp.Bridge.Data;

namespace WallApp.App.Services
{
    public delegate void PayloadHandler(IPayload payload);

    class BridgeMessageScheduler
    {
        private InputReader<IPayload> _reader;

        private Dictionary<Type, Queue<PayloadHandler>> _consumers;
        private bool _consumerLock;

        private bool _consumingNext;
        private IPayload _consumeNext;

        public BridgeMessageScheduler(InputReader<IPayload> reader)
        {
            _reader = reader;
            Task.Run(RunAsync);
        }

        public void TakeNext<T>(PayloadHandler action) where T : IPayload
        {
            while (_consumerLock) ;
            _consumerLock = true;

            Type tType = typeof(T);
            Queue<PayloadHandler> takersList = null;

            if(!_consumers.TryGetValue(tType, out takersList))
            {
                takersList = new Queue<PayloadHandler>();
                _consumers.Add(tType, takersList);
            }
            takersList.Enqueue(action);

            _consumerLock = false;
        }

        public T ConsumeNext<T>() where T : IPayload
        {
            _consumingNext = true;
            while (_consumeNext == null) ;

            var returnVal = (T)_consumeNext;
            _consumingNext = false;
            _consumeNext = null;

            return returnVal;
        }

        private void RunAsync()
        {

            while (true)
            {
                while (_reader.Queue.Count > 0)
                {
                    if(!_reader.Queue.TryDequeue(out var payload))
                    {
                        break;
                    }

                    if(!_consumerLock)
                    {
                        _consumerLock = true;
                        if(_consumers.TryGetValue(payload.GetType(), out var queue))
                        {
                            PayloadHandler handler = queue.Dequeue();
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
    }
}
