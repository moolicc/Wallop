using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public class PipeHost : IIpcEndpoint
    {
        public bool IsConnected { get; private set; }

        public int ServerCount { get; private set; }

        public string ResourceName { get; init; }

        public string ApplicationId { get; init; }


        private CancellationTokenSource? _cancelSource;
        private NamedPipeServerStream? _serverStream;

        private ConcurrentQueue<IpcMessage> _messages;

        public PipeHost(string applicationId, string resourceName)
        {
            ResourceName = resourceName;
            ApplicationId = applicationId;
            _messages = new ConcurrentQueue<IpcMessage>();
        }

        public void Begin()
        {
            ServerCount++;
            _cancelSource = new CancellationTokenSource();
            _serverStream = new NamedPipeServerStream(ResourceName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances);
            Task.Factory.StartNew(ListenAsync, _cancelSource.Token);
        }

        public bool Acquire(TimeSpan? timeout = null)
        {
            return true;
        }

        public bool DequeueMessage(out IpcMessage message)
        {
            return _messages.TryDequeue(out message);
        }

        public void QueueMessage(IpcMessage message)
        {
            _messages.Enqueue(message);
        }

        public void Release()
        {
        }

        private async Task ListenAsync()
        {
            if (_cancelSource == null || _serverStream == null)
            {
                return;
            }

            await _serverStream.WaitForConnectionAsync(_cancelSource.Token);
            if(_cancelSource.IsCancellationRequested)
            {
                return;
            }

            // Read the message.
            var incoming = ReadServerData();

            // Determine what to do based on type of message.
            var message = System.Text.Json.JsonSerializer.Deserialize<PipedMessage>(incoming);
            if(message.Type == PipedMessageTypes.QueueRequest)
            {
                if(message.Message.HasValue)
                {
                    _messages.Enqueue(message.Message.Value);
                }
                else
                {
                    // TODO: Error.
                }
            }
            else if(message.Type == PipedMessageTypes.DequeueRequest)
            {
                IpcMessage outgoingData;
                while (!_messages.TryDequeue(out outgoingData))
                {
                }

                var outgoing = new PipedMessage(PipedMessageTypes.DequeueRequest, outgoingData);
                var text = System.Text.Json.JsonSerializer.Serialize(outgoing);
                WriteServerData(text);
            }

            // Recreate server.
            Begin();
        }

        private string ReadServerData()
        {
            if(_serverStream == null)
            {
                return "error";
            }

            var data = new byte[4];
            _serverStream.Read(data, 0, 4);

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }

            var length = BitConverter.ToInt32(data);
            data = new byte[length];
            _serverStream.Read(data, 0, length);

            return Encoding.UTF8.GetString(data);
        }

        private void WriteServerData(string data)
        {
            if (_serverStream == null)
            {
                return;
            }

            var dataBuffer = Encoding.UTF8.GetBytes(data);
            var lenBuffer = BitConverter.GetBytes(dataBuffer.Length);
            var buffer = new byte[dataBuffer.Length + lenBuffer.Length];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lenBuffer);
            }

            Array.Copy(lenBuffer, buffer, lenBuffer.Length);
            Array.Copy(dataBuffer, 0, buffer, lenBuffer.Length, dataBuffer.Length);

            _serverStream.Write(buffer, 0, buffer.Length);
        }
    }
}
