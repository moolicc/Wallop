using System;
using System.Collections;
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

        private ConcurrentDictionary<string, ConcurrentQueue<IpcData>> _queues;

        public PipeHost(string applicationId, string resourceName)
            : base(applicationId, resourceName)
        {
            _queues = new ConcurrentDictionary<string, ConcurrentQueue<IpcData>>();
            Encoding = Encoding.ASCII;
            AllowMultipleClients = true;
        }


        public Task Listen(CancellationToken cancelToken)
        {
            _listenTask = ListenAsync(cancelToken);
            return _listenTask;
        }



        public override async Task<IpcData?> DequeueDataAsync(string queueName, CancellationToken? cancelToken = null)
        {
            var data = await Task.Run<IpcData?>(() =>
            {
                while (!cancelToken.HasValue || (cancelToken.HasValue && !cancelToken.Value.IsCancellationRequested))
                {
                    if (_queues.TryGetValue(queueName, out var queue))
                    {
                        if (queue.TryDequeue(out var data))
                        {
                            return data;
                        }
                    }
                    else
                    {
                        _queues.TryAdd(queueName, new ConcurrentQueue<IpcData>());
                    }
                }

                return null;
            }).ConfigureAwait(false);

            Console.WriteLine("[{0}] Dequeueing: {1}", ResourceName, data);
            return data;
        }

        public override Task<bool> QueueDataAsync(IpcData data, string queueName, CancellationToken? cancelToken = null)
        {
            Console.WriteLine("[{0}] Enqueuing: {1}", ResourceName, data);

            if(!_queues.TryGetValue(queueName, out var queue))
            {
                queue = new ConcurrentQueue<IpcData>();
                while(!_queues.TryAdd(queueName, queue));
            }
            queue.Enqueue(data);

            return Task.FromResult(true);
        }

        private async Task ListenAsync(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                _pipeServerStream = new NamedPipeServerStream(ResourceName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                PipesCreated++;

                await _pipeServerStream.WaitForConnectionAsync(cancelToken).ConfigureAwait(false);
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

                await HandleClientAsync(cancelToken).ConfigureAwait(false);
                //if(!AllowMultipleClients)
                //{
                //    await handler;
                //}
                _pipeServerStream.Disconnect();
                _pipeServerStream.Dispose();
            }
        }

        private async Task HandleClientAsync(CancellationToken cancelToken)
        {
            if(_pipeServerStream == null)
            {
                throw new InvalidOperationException("Pipe server is null.");
            }

            bool loop = true;

            try
            {
                while (loop)
                {
                    var buffer = new byte[4];
                    await _pipeServerStream.ReadAsync(buffer, 0, buffer.Length, cancelToken).ConfigureAwait(false);
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

                    await _pipeServerStream.ReadAsync(buffer, 0, length, cancelToken).ConfigureAwait(false);
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var textData = Encoding.GetString(buffer);
                    loop = await HandleDataAsync(textData).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async Task<bool> HandleDataAsync(string data)
        {
            // TODO: Handle lost connections.
            var datagram = Serializer.Deserialize<PipeDatagram>(data);
            var ipcData = datagram.IpcData;
            if(datagram.Command == PipeCommand.Enqueue)
            {
                if(ipcData != null)
                {
                    //var ipcData = Serializer.Deserialize<IpcData>(datagram.IpcData);
                    await QueueDataAsync(ipcData.Value, ipcData.Value.Packet.TargetApplication).ConfigureAwait(false);
                }
                return true;
            }
            else if(datagram.Command == PipeCommand.Dequeue)
            {
                ipcData = await DequeueDataAsync(ipcData.Value.Packet.SourceApplication).ConfigureAwait(false);
                if(ipcData != null)
                {
                    await WriteIpcResponse((IpcData)ipcData).ConfigureAwait(false);
                }
                return true;
            }

            return false;
        }


        private async Task WriteIpcResponse(IpcData data)
        {
            //var text = Serializer.Serialize(data);
            var datagram = new PipeDatagram(PipeCommand.DequeueResponse, data);

            var text = Serializer.Serialize(datagram);

            var outgoing = GetData(text);

            await _pipeServerStream!.WriteAsync(outgoing, 0, outgoing.Length).ConfigureAwait(false);
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
