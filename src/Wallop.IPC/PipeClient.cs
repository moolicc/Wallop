using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public class PipeClient : IIpcEndpoint
    {
        public bool IsConnected => _clientStream != null && _clientStream.IsConnected;

        public string ResourceName { get; init; }
        public string ApplicationId { get; init; }
        public string HostName { get; init; }


        private NamedPipeClientStream? _clientStream;


        public PipeClient(string applicationId, string resourceName, string hostName = ".")
        {
            ApplicationId = applicationId;
            ResourceName = resourceName;
            HostName = hostName;
        }

        public bool Acquire(TimeSpan? timeout = null)
        {
            if(!timeout.HasValue)
            {
                timeout = TimeSpan.FromSeconds(10);
            }

            try
            {
                _clientStream = new NamedPipeClientStream(".", ResourceName, PipeDirection.InOut);
                _clientStream.Connect(timeout.Value.Milliseconds);
            }
            catch
            {
                _clientStream?.Dispose();
                return false;
            }
            return true;
        }

        public bool DequeueMessage(out IpcMessage? message)
        {
            VerifyAcquired();
            try
            {
                var outgoing = new PipedMessage(PipedMessageTypes.DequeueRequest, null);
                var textData = System.Text.Json.JsonSerializer.Serialize(outgoing);
                WriteMessage(textData);


                textData = ReadMessage();
                message = System.Text.Json.JsonSerializer.Deserialize<IpcMessage>(textData);
            }
            catch (Exception ex)
            {
                message = null;
                return false;
            }
            return true;
        }

        public void QueueMessage(IpcMessage message)
        {
            VerifyAcquired();
            if (message.SourceApplication != ApplicationId)
            {
                throw new InvalidOperationException("Message source must be this Client.");
            }

            var outgoing = new PipedMessage(PipedMessageTypes.QueueRequest, message);
            var textData = System.Text.Json.JsonSerializer.Serialize(outgoing);
            WriteMessage(textData);
        }

        public void Release()
        {
            VerifyAcquired();

            var outgoing = new PipedMessage(PipedMessageTypes.Release, null);
            var textData = System.Text.Json.JsonSerializer.Serialize(outgoing);
            WriteMessage(textData);

            _clientStream?.Close();
            _clientStream?.Dispose();
        }

        private string ReadMessage()
        {
            return "";
        }

        private void WriteMessage(string data)
        {
            if (_clientStream == null)
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

            _clientStream.Write(buffer, 0, buffer.Length);
        }

        private void VerifyAcquired()
        {
            if(_clientStream == null || !_clientStream.IsConnected)
            {
                throw new InvalidOperationException("You must first acquire.");
            }
        }
    }
}
