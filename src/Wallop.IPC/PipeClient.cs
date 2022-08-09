using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public class PipeClient : IpcAgent
    {
        public readonly record struct PipeClientConfig(string ApplicationId, string HostMachine, string HostApplication, string ResourceName);

        public delegate Task UseClientAction(PipeClient client, CancellationToken cancelToken);


        public static readonly string LocalMachine = ".";


        public static async Task UseAsync(PipeClientConfig config, UseClientAction useAction)
            => await UseAsync(config.ApplicationId, config.HostMachine, config.HostApplication, config.ResourceName, useAction);

        public static async Task UseAsync(string applicationId, string hostMachine, string hostApplication, string resourceName, UseClientAction useAction)
        {
            var client = new PipeClient(applicationId, hostMachine, hostApplication, resourceName);
            var cancelSource = new CancellationTokenSource();
            await client.BeginAsync();

            await useAction(client, cancelSource.Token);

            await client.EndAsync();
            cancelSource.Dispose();
        }


        public Encoding Encoding { get; set; }
        public string HostApplication { get; private set; }
        public string HostMachine { get; private set; }

        private NamedPipeClientStream? _pipeClient;

        public PipeClient(PipeClientConfig conf)
            : this(conf.ApplicationId, conf.HostMachine, conf.HostApplication, conf.ResourceName)
        {
        }

        public PipeClient(string applicationId, string hostMachine, string hostApplication, string resourceName)
            : base(applicationId, resourceName)
        {
            HostMachine = hostMachine;
            HostApplication = hostApplication;
            Encoding = Encoding.ASCII;
        }

        public async Task BeginAsync()
        {
            _pipeClient = new NamedPipeClientStream(HostMachine, ResourceName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await _pipeClient.ConnectAsync().ConfigureAwait(false);
        }

        public async Task EndAsync()
        {
            if(_pipeClient?.IsConnected == true)
            {
                await SendDisconnectAsync().ConfigureAwait(false);
            }


            if(_pipeClient == null)
            {
                return;
            }
            _pipeClient.Close();
            await _pipeClient.DisposeAsync().ConfigureAwait(false);
        }

        public override async Task<bool> QueueDataAsync(IpcData data, CancellationToken? cancelToken)
        {
            if (_pipeClient == null)
            {
                return false;
            }

            try
            {
                var serialized = Serializer.Serialize(data);
                var datagram = new PipeDatagram(PipeCommand.Enqueue, serialized);

                var text = Serializer.Serialize(datagram);
                var buffer = GetBytes(text);
                await _pipeClient.WriteAsync(buffer, 0, buffer.Length, cancelToken ?? new CancellationToken(false)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public override async Task<IpcData?> DequeueDataAsync(CancellationToken? cancelToken)
        {
            if (_pipeClient == null)
            {
                return null;
            }

            try
            {
                var datagram = new PipeDatagram(PipeCommand.Dequeue, null);
                var outgoing = Serializer.Serialize(datagram);
                var buffer = GetBytes(outgoing);

                await _pipeClient.WriteAsync(buffer, 0, buffer.Length, cancelToken ?? new CancellationToken(false)).ConfigureAwait(false);

                var incoming = await ReadAsync(cancelToken ?? new CancellationToken(false)).ConfigureAwait(false);
                datagram = Serializer.Deserialize<PipeDatagram>(incoming);

                if (datagram.IpcData == null)
                {
                    throw new InvalidOperationException("Exepected response data.");
                }

                var ipcData = Serializer.Deserialize<IpcData>(datagram.IpcData);
                return ipcData;
            }
            catch (Exception ex)
            {
            }
            return null;
        }


        private async Task SendDisconnectAsync()
        {
            if(_pipeClient == null)
            {
                return;
            }
            try
            {
                var datagram = new PipeDatagram(PipeCommand.Disconnect, null);

                var text = Serializer.Serialize(datagram);
                var buffer = GetBytes(text);
                await _pipeClient.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }
        }

        private byte[] GetBytes(string text)
        {
            var buffer = Encoding.GetBytes(text);
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

        private async Task<string> ReadAsync(CancellationToken cancelToken)
        {
            if (_pipeClient == null)
            {
                return string.Empty;
            }

            var buffer = new byte[4];
            await _pipeClient.ReadAsync(buffer, 0, buffer.Length, cancelToken).ConfigureAwait(false);

            int length = 0;
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 4);
            }
            length = BitConverter.ToInt32(buffer);

            buffer = new byte[length];

            await _pipeClient.ReadAsync(buffer, 0, length, cancelToken).ConfigureAwait(false);

            var textData = Encoding.GetString(buffer);
            return textData;
        }
    }
}
