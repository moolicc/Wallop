using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public delegate void RequestComplete(Request result);
    public delegate object? HandleReply(Request request);

    public delegate object? OnRequestReceived(Request request);
    public delegate void OnStatementReceived(string sender, object content);

    public abstract class IpcAgent : IpcConnection
    {
        private const int MESSAGE_PROC_DELAY = 1000;

        public OnRequestReceived? RequestReceivedCallback;
        public OnStatementReceived? StatementReceivedCallback;

        private Task? _messageProcTask;
        private CancellationToken? _messageProcCancelToken;


        public IpcAgent(string applicationId, string resourceName)
            : base(applicationId, resourceName)
        {
        }

        public Task ProcessMessages(CancellationToken cancelToken)
        {
            _messageProcCancelToken = cancelToken;
            _messageProcTask = Task.Factory.StartNew(ProcessIncomingAsync, cancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            return _messageProcTask;
        }

        protected virtual int GetNextMessageId()
        {
            long start = 0;
            var yearStart = new DateTime(DateTime.Now.Year, 1, 1).Ticks;
            unchecked
            {
                start = DateTime.Now.Ticks;

                while (start >= int.MaxValue)
                {
                    start -= yearStart;
                    if (start < 0)
                    {
                        yearStart /= 2;
                        start = -start;
                    }
                }
            }
            return (int)start;
        }



        public virtual async Task<bool> SendStatementAsync<T>(T message, string targetApplication = "*")
            where T : notnull
        {
            var serialized = Serializer.Serialize(message);
            return await SendStatementAsync(serialized, targetApplication);
        }

        public virtual async Task<bool> SendStatementAsync(string message, string targetApplication = "*")
        {
            var data = new IpcMessage(MessageTypes.Statement, GetNextMessageId(), message, null);
            return await SendMessageAsync(data, targetApplication);
        }


        public virtual async Task<Request> SendRequestAsync<TRequest>(TRequest message, RequestComplete? requestComplete, string targetApplication = "*")
            where TRequest : notnull
        {
            var serialized = Serializer.Serialize(message);
            var outgoing = new IpcMessage(MessageTypes.Request, GetNextMessageId(), serialized, null);

            if (!await SendMessageAsync(outgoing, targetApplication))
            {
                var result = Request.Failed(Serializer, targetApplication, outgoing.MessageId, "Failed to send request message.");
                requestComplete?.Invoke(result);
                return result;
            }

            var incoming = await RecvMessageAsync(MessageTypes.Response);
            if(incoming == null)
            {
                var result = Request.Failed(Serializer, targetApplication, outgoing.MessageId, "Failed to receive response.");
                requestComplete?.Invoke(result);
                return result;
            }

            while (incoming.Value.ReplyToId != outgoing.MessageId)
            {
                incoming = await RecvMessageAsync(MessageTypes.Response);
                if (incoming == null)
                {
                    var result = Request.Failed(Serializer, targetApplication, outgoing.MessageId, "Failed to receive response on retry.");
                    requestComplete?.Invoke(result);
                    return result;
                }
            }

            var response = Request.Success(Serializer, targetApplication, outgoing.MessageId, incoming.Value.Message);
            requestComplete?.Invoke(response);
            return response;
        }

        public virtual async Task<T?> ReceiveStatementAsync<T>()
        {
            var incomingPacket = await RecvAsync();
            if (incomingPacket == null)
            {
                return default(T?);
            }

            var incomingMessage = Serializer.Deserialize<IpcMessage>(incomingPacket.Value.Content);
            if (incomingMessage.Type != MessageTypes.Statement)
            {
                await SendAsync(incomingPacket.Value);
                return default(T?);
            }

            var message = Serializer.Deserialize<T>(incomingMessage.Message);
            return message;
        }

        public virtual async Task<object?> ReceiveStatementAsync()
        {
            var incomingPacket = await RecvAsync();
            if (incomingPacket == null)
            {
                return null;
            }

            var incomingMessage = Serializer.Deserialize<IpcMessage>(incomingPacket.Value.Content);
            if (incomingMessage.Type != MessageTypes.Statement)
            {
                await SendAsync(incomingPacket.Value);
                return null;
            }

            var message = Serializer.Deserialize(incomingMessage.Message);
            return message;
        }

        public virtual async Task<Request?> RespondToRequestAsync(HandleReply? handleReply)
        {
            var incomingPacket = await RecvAsync();
            if(incomingPacket == null)
            {
                return null;
            }

            var incomingMessage = Serializer.Deserialize<IpcMessage>(incomingPacket.Value.Content);
            if(incomingMessage.Type != MessageTypes.Request)
            {
                await SendAsync(incomingPacket.Value);
                return null;
            }

            var result = Request.Success(Serializer, incomingPacket.Value.SourceApplication, incomingMessage.MessageId, incomingMessage.Message);
            var replyData = handleReply?.Invoke(result);

            if(replyData != null)
            {
                var serialized = Serializer.Serialize(replyData);
                var outgoing = new IpcMessage(MessageTypes.Response, GetNextMessageId(), serialized, incomingMessage.MessageId);
                await SendMessageAsync(outgoing, incomingPacket.Value.SourceApplication);
            }

            return result;
        }



        public virtual async Task<bool> SendMessageAsync(IpcMessage message, string targetApplication)
        {
            var serialized = Serializer.Serialize(message);
            var packet = new IpcPacket(serialized, ApplicationId, targetApplication);
            return await SendAsync(packet);
        }

        public virtual async Task<IpcMessage?> RecvMessageAsync(MessageTypes typeFilter)
        {
            var packet = await RecvAsync();
            if(packet == null)
            {
                return null;
            }

            var ipcMessage = Serializer.Deserialize<IpcMessage>(packet.Value.Content);
            if(!typeFilter.HasFlag(ipcMessage.Type))
            {
                await SendAsync(packet.Value);
                return null;
            }

            return ipcMessage;
        }

        private async Task ProcessIncomingAsync()
        {
            while (_messageProcCancelToken != null && !_messageProcCancelToken.Value.IsCancellationRequested)
            {
                if(RequestReceivedCallback != null || StatementReceivedCallback != null)
                {
                    var incoming = await RecvAsync(TimeSpan.FromSeconds(1));
                    if(incoming != null)
                    {
                        var ipcMessage = Serializer.Deserialize<IpcMessage>(incoming.Value.Content);

                        if(incoming.Value.TargetApplication != AnyApplication && incoming.Value.TargetApplication != ApplicationId)
                        {
                            await SendAsync(incoming.Value);
                        }
                        else if(ipcMessage.Type == MessageTypes.Request && RequestReceivedCallback != null)
                        {
                            var replyData = RequestReceivedCallback(Request.Success(Serializer, incoming.Value.SourceApplication, ipcMessage.MessageId, ipcMessage.Message));
                            
                            if (replyData != null)
                            {
                                var serialized = Serializer.Serialize(replyData);
                                var outgoing = new IpcMessage(MessageTypes.Response, GetNextMessageId(), serialized, ipcMessage.MessageId);
                                await SendMessageAsync(outgoing, incoming.Value.SourceApplication);
                            }
                        }
                        else if(ipcMessage.Type == MessageTypes.Statement && StatementReceivedCallback != null)
                        {
                            StatementReceivedCallback(incoming.Value.SourceApplication, Serializer.Deserialize(ipcMessage.Message));
                        }
                        else
                        {
                            await SendAsync(incoming.Value);
                        }
                    }
                }

                await Task.Delay(MESSAGE_PROC_DELAY);
            }
        }
    }
}
