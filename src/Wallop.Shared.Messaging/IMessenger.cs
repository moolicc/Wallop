using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging
{
    public delegate void MessageHook(uint[] messageIds, ValueType[] messages, Type messageType);
    public delegate void MessageListener<T>(MessageProxy<T> message, ref bool handled) where T : struct;

    public interface IMessenger
    {
        void RegisterQueue<T>() where T : struct;
        bool Take(out ValueType? payload, Type targetType, ref uint messageId);
        bool Take<T>(ref T payload, ref uint messageId) where T : struct;
        MessageProxy<T>[] Take<T>(int count) where T : struct;
        MessageProxy<T>[] Take<T>(ref int count) where T : struct;

        uint Put(ValueType message, Type messageType);
        uint Put<T>(T message) where T : struct;

        void AddTakeHook(MessageHook hook);
        void RemoveTakeHook(MessageHook hook);
        void AddPutHook(MessageHook hook);
        void RemovePutHook(MessageHook hook);
        void Listen<T>(MessageListener<T> listener) where T : struct;
        void ClearState();
    }
}
