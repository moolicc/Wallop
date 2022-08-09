using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.IPC.Serialization;

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


        public virtual async Task<bool> SendStatementAsync(string message, string targetApplication = "*")
        {
            var intermediate = IntermediateValue.FromSerialized(Serializer, message);
            var data = new IpcMessage(MessageTypes.Statement, GetNextMessageId(), intermediate, null);
            return await SendMessageAsync(data, targetApplication).ConfigureAwait(false);
        }


        public virtual async Task<Request> SendRequestAsync<TRequest>(TRequest message, RequestComplete? requestComplete, string targetApplication = "*")
            where TRequest : notnull
        {
            var intermediate = IntermediateValue.FromSerialized(Serializer, message);
            var outgoing = new IpcMessage(MessageTypes.Request, GetNextMessageId(), intermediate, null);

            if (!await SendMessageAsync(outgoing, targetApplication).ConfigureAwait(false))
            {
                var result = Request.Failed(Serializer, targetApplication, outgoing.MessageId, "Failed to send request message.");
                requestComplete?.Invoke(result);
                return result;
            }

            var incoming = await RecvMessageAsync(MessageTypes.Response).ConfigureAwait(false);
            if(incoming == null)
            {
                var result = Request.Failed(Serializer, targetApplication, outgoing.MessageId, "Failed to receive response.");
                requestComplete?.Invoke(result);
                return result;
            }

            while (incoming.Value.ReplyToId != outgoing.MessageId)
            {
                incoming = await RecvMessageAsync(MessageTypes.Response).ConfigureAwait(false);
                if (incoming == null)
                {
                    var result = Request.Failed(Serializer, targetApplication, outgoing.MessageId, "Failed to receive response on retry.");
                    requestComplete?.Invoke(result);
                    return result;
                }
            }

            var response = Request.Success(Serializer, targetApplication, outgoing.MessageId, incoming.Value.Content.Value);
            requestComplete?.Invoke(response);
            return response;
        }

        public virtual async Task<T?> ReceiveStatementAsync<T>()
        {
            var incomingPacket = await RecvAsync().ConfigureAwait(false);
            if (incomingPacket == null)
            {
                return default(T?);
            }

            //var incomingMessage = Serializer.Deserialize<IpcMessage>(incomingPacket.Value.Content);
            var incomingMessage = incomingPacket.Value.Message;
            if (incomingMessage.Type != MessageTypes.Statement)
            {
                await SendAsync(incomingPacket.Value).ConfigureAwait(false);
                return default(T?);
            }

            //var message = Serializer.Deserialize<T>(incomingMessage.Message);

            // TODO: If this works, Add T constraint and avoid this cast.
            return (T)incomingMessage.Content.Value;
        }

        public virtual async Task<object?> ReceiveStatementAsync()
        {
            var incomingPacket = await RecvAsync().ConfigureAwait(false);
            if (incomingPacket == null)
            {
                return null;
            }

            //var incomingMessage = Serializer.Deserialize<IpcMessage>(incomingPacket.Value.Content);
            var incomingMessage = incomingPacket.Value.Message;
            if (incomingMessage.Type != MessageTypes.Statement)
            {
                await SendAsync(incomingPacket.Value).ConfigureAwait(false);
                return null;
            }

            //var message = Serializer.Deserialize(incomingMessage.Message);

            return incomingMessage.Content;
        }

        public virtual async Task<Request?> RespondToRequestAsync(HandleReply? handleReply)
        {
            var incomingPacket = await RecvAsync().ConfigureAwait(false);
            if(incomingPacket == null)
            {
                return null;
            }

            //var incomingMessage = Serializer.Deserialize<IpcMessage>(incomingPacket.Value.Content);
            var incomingMessage = incomingPacket.Value.Message;
            if (incomingMessage.Type != MessageTypes.Request)
            {
                await SendAsync(incomingPacket.Value).ConfigureAwait(false);
                return null;
            }

            var result = Request.Success(Serializer, incomingPacket.Value.SourceApplication, incomingMessage.MessageId, incomingMessage.Content);
            var replyData = handleReply?.Invoke(result);

            if(replyData != null)
            {
                var intermediate = IntermediateValue.FromSerialized(Serializer, replyData);
                var outgoing = new IpcMessage(MessageTypes.Response, GetNextMessageId(), intermediate, incomingMessage.MessageId);
                await SendMessageAsync(outgoing, incomingPacket.Value.SourceApplication).ConfigureAwait(false);
            }

            return result;
        }



        public virtual async Task<bool> SendMessageAsync(IpcMessage message, string targetApplication)
        {
            var serialized = Serializer.Serialize(message);
            //var packet = new IpcPacket(serialized, ApplicationId, targetApplication);
            var packet = new IpcPacket(message, ApplicationId, targetApplication);
            return await SendAsync(packet).ConfigureAwait(false);
        }

        public virtual async Task<IpcMessage?> RecvMessageAsync(MessageTypes typeFilter)
        {
            var packet = await RecvAsync().ConfigureAwait(false);
            if(packet == null)
            {
                return null;
            }

            //var ipcMessage = Serializer.Deserialize<IpcMessage>(packet.Value.Content);
            var ipcMessage = packet.Value.Message;
            if (!typeFilter.HasFlag(ipcMessage.Type))
            {
                await SendAsync(packet.Value).ConfigureAwait(false);
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
                    var incoming = await RecvAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                    if(incoming != null)
                    {
                        //var ipcMessage = Serializer.Deserialize<IpcMessage>(incoming.Value.Content);
                        var ipcMessage = incoming.Value.Message;

                        if (incoming.Value.TargetApplication != AnyApplication && incoming.Value.TargetApplication != ApplicationId)
                        {
                            await SendAsync(incoming.Value).ConfigureAwait(false);
                        }
                        else if(ipcMessage.Type == MessageTypes.Request && RequestReceivedCallback != null)
                        {
                            //var replyData = RequestReceivedCallback(Request.Success(Serializer, incoming.Value.SourceApplication, ipcMessage.MessageId, ipcMessage.Message
                            var replyData = RequestReceivedCallback(Request.Success(Serializer, incoming.Value.SourceApplication, ipcMessage.MessageId, ipcMessage.Content.Value));

                            if (replyData != null)
                            {
                                //var serialized = Serializer.Serialize(replyData);
                                //var outgoing = new IpcMessage(MessageTypes.Response, GetNextMessageId(), serialized, ipcMessage.MessageId);
                                var intermediate = IntermediateValue.FromSerialized(Serializer, replyData);
                                var outgoing = new IpcMessage(MessageTypes.Response, GetNextMessageId(), intermediate, ipcMessage.MessageId);
                                await SendMessageAsync(outgoing, incoming.Value.SourceApplication).ConfigureAwait(false);
                            }
                        }
                        else if(ipcMessage.Type == MessageTypes.Statement && StatementReceivedCallback != null)
                        {
                            //StatementReceivedCallback(incoming.Value.SourceApplication, Serializer.Deserialize(ipcMessage.Message));
                            StatementReceivedCallback(incoming.Value.SourceApplication, ipcMessage.Content.Value);
                        }
                        else
                        {
                            await SendAsync(incoming.Value).ConfigureAwait(false);
                        }
                    }
                }

                await Task.Delay(MESSAGE_PROC_DELAY).ConfigureAwait(false);
            }
        }
    }
}
