using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Messaging
{
    public class Messenger : IMessenger
    {
        private Dictionary<Type, IMessageQueue> _queues;
        private ushort _nextMessageId;
        private List<MessageHook> _putHooks;
        private List<MessageHook> _takeHooks;

        public Messenger()
        {
            _queues = new Dictionary<Type, IMessageQueue>();
            _nextMessageId = 1;
            _putHooks = new List<MessageHook>();
            _takeHooks = new List<MessageHook>();
        }

        public void RegisterQueue<T>() where T : struct
        {
            var queue = new MessageQueue<T>();
            _queues.Add(typeof(T), queue);
        }

        public bool Take<T>(ref T payload, ref uint messageId) where T : struct
        {
            if (!_queues.TryGetValue(typeof(T), out var queue))
            {
                queue = new MessageQueue<T>();
                _queues.Add(typeof(T), queue);
            }

            if (queue is MessageQueue<T> msgQueue)
            {
                bool succeeded = msgQueue.TakeNext(out var result) == TakeResults.Succeeded;

                if (!succeeded)
                {
                    return false;
                }

                payload = result.Payload;
                messageId = result.MessageId;

                RunTakeHooks(messageId, payload, typeof(T));
                return true;
            }
            return false;
        }

        public MessageProxy<T>[] Take<T>(int count) where T : struct
        {
            return Take<T>(ref count);
        }

        public MessageProxy<T>[] Take<T>(ref int count) where T : struct
        {
            const int MAX_FAILURES = 5;

            var buffer = new MessageProxy<T>[count];
            if (!_queues.TryGetValue(typeof(T), out var queue))
            {
                queue = new MessageQueue<T>();
                _queues.Add(typeof(T), queue);
            }

            if (queue is MessageQueue<T> msgQueue)
            {
                int failures = 0;
                int attemptCount = count;
                count = 0;
                for (int i = 0; i < attemptCount; i++)
                {
                    var result = msgQueue.TakeNext(out buffer[i]);
                    if (result == TakeResults.OutOfElements)
                    {
                        break;
                    }
                    if (result == TakeResults.Failed)
                    {
                        failures++;
                        i--;
                    }
                    if (failures >= MAX_FAILURES)
                    {
                        break;
                    }
                    count++;
                }
            }

            return buffer;
        }

        public bool Take([NotNullWhen(true)] out ValueType? payload, Type messageType, ref uint messageId)
        {
            payload = null;

            var constructedType = typeof(MessageQueue<>).MakeGenericType(messageType);
            if (!_queues.TryGetValue(messageType, out var queue))
            {
                return false;
            }

            var proxyType = typeof(MessageProxy<>).MakeGenericType(messageType);
            var proxyInstance = Activator.CreateInstance(proxyType);
            try
            {
                var methods = constructedType.GetMethods().Where(m => m.Name == nameof(MessageQueue<int>.TakeNext));
                MethodInfo? method = null;

                foreach (var item in methods)
                {
                    var param = item.GetParameters();
                    if (param.Length == 1)
                    {
                        if (param[0].ParameterType == proxyType)
                        {
                            method = item;
                            break;
                        }
                    }
                }

                if (method == null)
                {
                    throw new KeyNotFoundException("Failed to find expected Enqueue method.");
                }
                var result = (TakeResults)(method.Invoke(queue, new[] { proxyInstance }) ?? TakeResults.Failed);

                if(result == TakeResults.OutOfElements
                    || result == TakeResults.Failed)
                {
                    return false;
                }

                var property = proxyType.GetProperty("Payload")!;
                var propertyValue = property.GetValue(proxyInstance)!;
                payload = (ValueType)propertyValue;

                property = proxyType.GetProperty("MessageId")!;
                propertyValue = property.GetValue(proxyInstance)!;
                messageId = (uint)propertyValue;

                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public uint Put(ValueType message, Type messageType)
        {
            var constructedType = typeof(MessageQueue<>).MakeGenericType(messageType);
            if (!_queues.TryGetValue(messageType, out var queue))
            {
                var newInstance = Activator.CreateInstance(constructedType);

                if (newInstance == null)
                {
                    throw new InvalidCastException("Specified message type is invalid.");
                }

                queue = (IMessageQueue)newInstance;
                _queues.Add(messageType, queue);
            }

            uint msgId;
            ushort high;
            unchecked
            {
                high = _nextMessageId++;
            }

            try
            {
                var methods = constructedType.GetMethods().Where(m => m.Name == nameof(MessageQueue<int>.Enqueue));
                MethodInfo? method = null;

                foreach (var item in methods)
                {
                    var param = item.GetParameters();
                    if (param.Length == 2)
                    {
                        if (param[0].ParameterType == messageType
                            && param[1].ParameterType == typeof(uint))
                        {
                            method = item;
                            break;
                        }
                    }
                }

                if (method == null)
                {
                    throw new KeyNotFoundException("Failed to find expected Enqueue method.");
                }
                msgId = (uint)(method.Invoke(queue, new[] { message, high }) ?? 0);
            }
            catch (Exception)
            {
                return 0;
            }

            RunPutHooks(msgId, message, messageType);
            return msgId;
        }

        public uint Put<T>(T message) where T : struct
        {
            if (!_queues.TryGetValue(typeof(T), out var queue))
            {
                queue = new MessageQueue<T>();
                _queues.Add(typeof(T), queue);
            }

            uint msgId;
            ushort high;
            unchecked
            {
                high = _nextMessageId++;
            }

            if (queue is MessageQueue<T> msgQueue)
            {
                msgId = msgQueue.Enqueue(message, high);
            }
            else
            {
                return 0;
            }

            RunPutHooks(msgId, message, typeof(T));
            return msgId;
        }

        internal uint Put<T>(T message, uint preferredId) where T : struct
        {
            if (!_queues.TryGetValue(typeof(T), out var queue))
            {
                queue = new MessageQueue<T>();
                _queues.Add(typeof(T), queue);
            }

            if (queue is MessageQueue<T> msgQueue)
            {
                preferredId = msgQueue.Enqueue(message, preferredId);
            }
            else
            {
                return 0;
            }

            RunPutHooks(preferredId, message, typeof(T));
            return preferredId;
        }


        public void AddPutHook(MessageHook hook)
        {
            _putHooks.Add(hook);
        }

        public void RemovePutHook(MessageHook hook)
        {
            _putHooks.Remove(hook);
        }
        public void AddTakeHook(MessageHook hook)
        {
            _takeHooks.Add(hook);
        }

        public void RemoveTakeHook(MessageHook hook)
        {
            _takeHooks.Remove(hook);
        }

        public void Listen<T>(MessageListener<T> listener) where T : struct
        {
            if (!_queues.TryGetValue(typeof(T), out var queue))
            {
                queue = new MessageQueue<T>();
                _queues.Add(typeof(T), queue);
            }
            if (queue is MessageQueue<T> msgQueue)
            {
                msgQueue.MessageListener += listener;
            }
        }

        public void ClearState()
        {
            foreach (var queue in _queues)
            {
                queue.Value.ClearState();
            }
        }

        private void RunPutHooks(uint messageId, ValueType message, Type messageType)
        {
            foreach (var item in _putHooks)
            {
                item(new[] { messageId }, new[] { message }, messageType);
            }
        }

        private void RunTakeHooks(uint messageId, ValueType message, Type messageType)
        {
            foreach (var item in _takeHooks)
            {
                item(new[] { messageId }, new[] { message }, messageType);
            }
        }

        private void RunTakeHooks<T>(MessageProxy<T>[] buffer) where T : struct
        {
            var ids = new uint[buffer.Length];
            var values = new ValueType[buffer.Length];

            for (int i = 0; i < buffer.Length; i++)
            {
                ids[i] = buffer[i].MessageId;
                values[i] = buffer[i].Payload;
            }

            foreach (var item in _takeHooks)
            {
                item(ids, values, typeof(T));
            }
        }
    }
}
