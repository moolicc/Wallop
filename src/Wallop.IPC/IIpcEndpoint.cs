﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.IPC
{
    public interface IIpcEndpoint
    {
        string ResourceName { get; }
        string ApplicationId { get; }


        Task<bool> QueueDataAsync(IpcData data, string queueName, CancellationToken? cancelToken = null);
        Task<IpcData?> DequeueDataAsync(string queueName, CancellationToken? cancelToken = null);
    }
}
