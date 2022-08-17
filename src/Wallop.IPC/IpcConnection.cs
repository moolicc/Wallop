﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.IPC.Serialization;

namespace Wallop.IPC
{
    public abstract class IpcConnection : IIpcEndpoint
    {
        public static readonly string AnyApplication = "*";


        public bool IsConnected { get; protected set; }

        public string ResourceName { get; init; }

        public string ApplicationId { get; init; }

        public ISerializer Serializer { get; set; }


        protected IpcConnection(string applicationId, string resourceName)
        {
            ApplicationId = applicationId;
            ResourceName = resourceName;
            Serializer = new Json();
        }



        protected virtual async Task<bool> SendAsync(IpcPacket packet)
        {
            //var serialized = Serializer.Serialize(packet);
            var data = new IpcData(packet);
            return await QueueDataAsync(data).ConfigureAwait(false);
        }

        protected virtual async Task<IpcPacket?> RecvAsync(TimeSpan? timeout = null)
        {
            IpcPacket? packet = null;

            var failedTasks = new List<Task>(3);

            var cancelSource = new CancellationTokenSource();

            if (timeout != null)
            {
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(timeout.Value, cancelSource.Token).ConfigureAwait(false);
                    if (cancelSource.IsCancellationRequested)
                    {
                        return;
                    }
                    cancelSource.Cancel();
                }, cancelSource.Token);
            }



            while (!cancelSource.Token.IsCancellationRequested)
            {
                var received = await DequeueDataAsync(cancelSource.Token).ConfigureAwait(false);
                if (received == null)
                {
                    break;
                }

                //packet = Serializer.Deserialize<IpcPacket>(received.Value.Content);
                packet = received.Value.Packet;
                if (packet.Value.TargetApplication != ApplicationId && packet.Value.TargetApplication != AnyApplication)
                {
                    failedTasks.Add(SendAsync(packet.Value));
                }
                else
                {
                    break;
                }
            }

            if (!cancelSource.IsCancellationRequested)
            {
                cancelSource.Cancel();
                cancelSource.Dispose();
            }

            return packet;
        }

        public abstract Task<bool> QueueDataAsync(IpcData data, CancellationToken? cancelToken = null);

        public abstract Task<IpcData?> DequeueDataAsync(CancellationToken? cancelToken = null);
    }
}