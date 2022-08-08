using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public class PipeHost : IpcAgent
    {
        public static readonly string LocalMachine = ".";

        public Encoding Encoding { get; set; }
        public bool AllowMultipleClients { get; set; }

        public int PipesCreated { get; private set; }

        private NamedPipeServerStream? _pipeServerStream;
        private Task? _listenTask;

        private ConcurrentQueue<IpcData> _queue;

        public PipeHost(string applicationId, string resourceName)
            : base(applicationId, resourceName)
        {
            _queue = new ConcurrentQueue<IpcData>();
            Encoding = Encoding.ASCII;
            AllowMultipleClients = true;
        }


        public Task Listen(CancellationToken cancelToken)
        {
            _listenTask = ListenAsync(cancelToken);
            return _listenTask;
        }



        public override async Task<IpcData?> DequeueDataAsync(CancellationToken? cancelToken = null)
        {
            var data = await Task.Run<IpcData?>(() =>
            {
                while (!cancelToken.HasValue || (cancelToken.HasValue && !cancelToken.Value.IsCancellationRequested))
                {
                    if(_queue.TryDequeue(out var data))
                    {
                        return data;
                    }
                }

                return null;
            });

            return data;
        }

        public override Task<bool> QueueDataAsync(IpcData data, CancellationToken? cancelToken = null)
        {
            _queue.Enqueue(data);
            return Task.FromResult(true);
        }

        private async Task ListenAsync(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                _pipeServerStream = new NamedPipeServerStream(ResourceName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                PipesCreated++;

                await _pipeServerStream.WaitForConnectionAsync(cancelToken);
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

                var handler = HandleClientAsync(cancelToken);
                if(!AllowMultipleClients)
                {
                    await handler;
                }
            }
        }

        private async Task HandleClientAsync(CancellationToken cancelToken)
        {
            if(_pipeServerStream == null)
            {
                throw new InvalidOperationException("Pipe server is null.");
            }

            bool loop = true;

            while(loop)
            {
                var buffer = new byte[4];
                await _pipeServerStream.ReadAsync(buffer, 0, buffer.Length, cancelToken);
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                int length = 0;
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(buffer, 0, 4);
                }
                length = BitConverter.ToInt32(buffer);

                buffer = new byte[length];

                await _pipeServerStream.ReadAsync(buffer, 0, length, cancelToken);
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                var textData = Encoding.GetString(buffer);
                loop = await HandleDataAsync(textData);
            }
        }

        private async Task<bool> HandleDataAsync(string data)
        {
            // TODO: Handle lost connections.
            var datagram = Serializer.Deserialize<PipeDatagram>(data);
            
            if(datagram.Command == PipeCommand.Enqueue)
            {
                if(datagram.IpcData != null)
                {
                    var ipcData = Serializer.Deserialize<IpcData>(datagram.IpcData);
                    await QueueDataAsync(ipcData);
                }
                return true;
            }
            else if(datagram.Command == PipeCommand.Dequeue)
            {
                var ipcData = await DequeueDataAsync();
                if(ipcData != null)
                {
                    await WriteIpcResponse(ipcData.Value);
                }
                return true;
            }

            return false;
        }


        private async Task WriteIpcResponse(IpcData data)
        {
            var text = Serializer.Serialize(data);
            var datagram = new PipeDatagram(PipeCommand.DequeueResponse, text);

            text = Serializer.Serialize(datagram);

            var outgoing = GetData(text);

            await _pipeServerStream!.WriteAsync(outgoing, 0, outgoing.Length);
        }

        private byte[] GetData(string textData)
        {
            var buffer = Encoding.GetBytes(textData);
            var length = BitConverter.GetBytes(buffer.Length);

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(length);
            }

            var returnVal = new byte[buffer.Length + length.Length];
            Array.Copy(length, 0, returnVal, 0, length.Length);
            Array.Copy(buffer, 0, returnVal, length.Length, buffer.Length);

            return returnVal;
        }
    }
}
