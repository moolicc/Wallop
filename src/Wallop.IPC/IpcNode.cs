using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public class IpcNode
    {
        private const int TICK_TIME = 100;
        private const int ACQUIRE_RECV_TIME = 1000;

        public Action<IpcNode, string>? OnDataReceived;
        public Action<IpcNode, IpcMessage>? OnDataReceived2;

        public IIpcEndpoint Endpoint { get; init; }

        Task _listenTask;
        private CancellationTokenSource _cancelSource;

        private bool _awaitingReply;
        private string? _pendingReply;

        public IpcNode(IIpcEndpoint endpoint)
        {
            Endpoint = endpoint;
            OnDataReceived = null;
            OnDataReceived2 = null;
            _awaitingReply = false;
            _pendingReply = null;
            _cancelSource = new CancellationTokenSource();

            _listenTask = Task.Factory.StartNew(WatchEndpoint, _cancelSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Shutdown()
        {
            _cancelSource.Cancel();
            _listenTask.Wait();

            _listenTask.Dispose();
            _cancelSource.Dispose();
        }

        public void Send(string message, string targetApplication = "*")
        {
            if(!Endpoint.Acquire())
            {
                throw new InvalidOperationException("Failed to acquire Endpoint lock.");
            }

            var data = new IpcMessage(GetNewMessageId(), message, Endpoint.ApplicationId, targetApplication);
            Endpoint.QueueMessage(data);
            Endpoint.Release();
        }

        public bool GetReply(TimeSpan timeout, [NotNullWhen(true)] out string? reply)
        {
            var start = DateTime.Now;

            _awaitingReply = true;
            while (timeout > DateTime.Now - start && _pendingReply == null)
            {
            }

            if(_pendingReply == null)
            {
                _awaitingReply = false;
                reply = null;
                return false;
            }
            
            reply = _pendingReply;
            _pendingReply = null;
            _awaitingReply = false;
            return true;
        }

        private int GetNewMessageId()
        {
            var todayTicks = DateTime.Today.Ticks;
            double diff = DateTime.Now.Ticks - todayTicks;

            while(diff > int.MaxValue)
            {
                diff /= 2;
            }

            return (int)diff;
        }

        private void WatchEndpoint()
        {
            while (!_cancelSource.IsCancellationRequested)
            {
                try
                {
                    if((OnDataReceived != null || OnDataReceived2 != null || _awaitingReply) && Endpoint.Acquire(TimeSpan.FromMilliseconds(ACQUIRE_RECV_TIME)))
                    {
                        int startId = -1;
                    
                        while (!_cancelSource.IsCancellationRequested && Endpoint.DequeueMessage(out var message) && message.Value.MessageId != startId)
                        {
                            if (startId == -1)
                            {
                                startId = message.Value.MessageId;
                            }

                            if(message.Value.TargetApplication != "*" && message.Value.TargetApplication != Endpoint.ApplicationId)
                            {
                                Endpoint.Release();

                                Endpoint.Acquire();
                                Endpoint.QueueMessage(message.Value);
                                Endpoint.Release();
                                continue;
                            }
                            Endpoint.Release();

                            try
                            {
                                if (_awaitingReply)
                                {
                                    _pendingReply = message.Value.Content;
                                    _awaitingReply = false;
                                }

                                if(OnDataReceived2 != null)
                                {
                                    OnDataReceived2(this, message.Value);
                                }
                                if(OnDataReceived != null)
                                {
                                    OnDataReceived(this, message.Value.Content);
                                }
                            }
                            catch (Exception ex)
                            {

                                throw;
                            }

                            break;
                        }

                        if(startId == -1)
                        {
                            Endpoint.Release();
                        }
                    }
                }
                catch (Exception)
                {
                }

                if (!_cancelSource.IsCancellationRequested)
                {
                    Thread.Sleep(TICK_TIME);
                }
            }
        }
    }
}
