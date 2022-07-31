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
        bool IsConnected { get; }
        string ResourceName { get; }
        string ApplicationId { get; }


        bool Acquire(TimeSpan? timeout = null);
        void QueueMessage(IpcMessage message);
        bool DequeueMessage([NotNullWhen(true)] out IpcMessage? message);
        void Release();
    }
}