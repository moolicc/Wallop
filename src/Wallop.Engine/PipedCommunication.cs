using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Wallop
{
    /*
     * Protocol:
     * <header><nl><message><nl>
     * 
     * <header> := <linecount>
     * <message> := <string> | <string><nl><message>
     * 
     * <linecount> := <digit>
     * <nl> := \n
     * 
     * The linecount is the number of lines the message consists of.
     */
    public class PipedCommunication : IDisposable
    {
        private const string SERVER_PIPE_NAME = "WallopPipe";

        public bool IsServer { get; init; }
        public Action<string>? MessageReceivedCallback;

        private CancellationTokenSource? _cancelSource;
        private NamedPipeServerStream? _serverStream;
        private StringBuilder _messageBuilder;

        private NamedPipeClientStream? _clientStream;

        private uint _streamsCreated;

        private PipedCommunication(bool isServer, string host)
        {
            IsServer = isServer;
            _messageBuilder = new StringBuilder();

            if(isServer)
            {
                _streamsCreated = 0;
                CreateServerResources();
            }
            else
            {
                EngineLog.For<PipedCommunication>().Info("Spinning up client stream associated with server: {name} on host {host}.", SERVER_PIPE_NAME, host);
                _clientStream = new NamedPipeClientStream(".", SERVER_PIPE_NAME, PipeDirection.Out);
            }
        }

        public static PipedCommunication CreateServer()
            => new PipedCommunication(true, "");

        public static PipedCommunication CreateClient(string host = ".")
            => new PipedCommunication(false, host);


        private void CreateServerResources()
        {
            _streamsCreated++;
            EngineLog.For<PipedCommunication>().Info("Spinning up server stream with name: {name}...", SERVER_PIPE_NAME);
            EngineLog.For<PipedCommunication>().Info("Total servers created: {servers}", _streamsCreated);
            _cancelSource = new CancellationTokenSource();
            _serverStream = new NamedPipeServerStream(SERVER_PIPE_NAME, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances);
            Task.Factory.StartNew(ListenAsync, _cancelSource.Token);
        }

        public void SendMessageToServer(string message)
        {
            EngineLog.For<PipedCommunication>().Info("Sending message to server...");
            if (_clientStream == null)
            {
                EngineLog.For<PipedCommunication>().Fatal("Failed to send message! Client stream null!");
                return;
            }
            int lineCount = 1;
            for (int i = 0; i < message.Length; i++)
            {
                if(message[i] == '\n')
                {
                    lineCount++;
                }
            }


            EngineLog.For<PipedCommunication>().Info("Connecting to server...");
            _clientStream.Connect();

            if(!_clientStream.IsConnected)
            {
                EngineLog.For<PipedCommunication>().Fatal("Client failed to connect to server!");
                return;
            }

            using (var writer = new StreamWriter(_clientStream))
            {
                EngineLog.For<PipedCommunication>().Info("Writing {lines} lines from message of length {len} to server.", lineCount, message.Length);
                EngineLog.For<PipedCommunication>().Info("{msg}", message);
                writer.WriteLine(lineCount);
                writer.WriteLine(message);
                writer.Flush();
            }
        }

        public void ShutdownServer()
        {
            EngineLog.For<PipedCommunication>().Info("Server shutting down...");
            _cancelSource?.Cancel();
            _serverStream?.Disconnect();
        }

        private async Task ListenAsync()
        {
            if(_cancelSource == null || _serverStream == null)
            {
                return;
            }

            EngineLog.For<PipedCommunication>().Info("Server listening for connections...");
            await _serverStream.WaitForConnectionAsync(_cancelSource.Token);
            if(_cancelSource.IsCancellationRequested)
            {
                return;
            }

            EngineLog.For<PipedCommunication>().Info("Server reading message...");
            var message = ReadServerMessage().Trim();

            EngineLog.For<PipedCommunication>().Info("Server invoking message received callback for message of length {len}...", message.Length);
            EngineLog.For<PipedCommunication>().Info("{msg}", message);
            await Task.Factory.StartNew(() => MessageReceivedCallback?.Invoke(message));

            CreateServerResources();
        }

        private string ReadServerMessage()
        {
            if(_serverStream == null)
            {
                EngineLog.For<PipedCommunication>().Fatal("Server null for some reason!");
                return "";
            }

            int lines = 0;
            _messageBuilder.Clear();

            using (var reader = new StreamReader(_serverStream))
            {
                var lengthLine = reader.ReadLine();
                if (!int.TryParse(lengthLine, out lines))
                {
                    EngineLog.For<PipedCommunication>().Error("Server failed to read message line count!", lengthLine);
                    return "";
                }
                EngineLog.For<PipedCommunication>().Info("Server reading {lines} lines from stream...", lengthLine);

                for (int i = 0; i < lines; i++)
                {
                    _messageBuilder.AppendLine(reader.ReadLine());
                }
            }

            return _messageBuilder.ToString();
        }

        public void Dispose()
        {
            EngineLog.For<PipedCommunication>().Info("Cleaning up...");
            MessageReceivedCallback = null;
            _clientStream?.Dispose();
            _serverStream?.Dispose();
            _cancelSource?.Dispose();
        }
    }
}
