using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Messaging
{
    public delegate void MessageHandler<T>(T message, uint messageId) where T : struct;

    public class MessageDelegateDispatcher<T> : IMessageDispatcher
        where T : struct
    {
        public int MessagesPerDispatch { get; set; }
        public MessageHandler<T> Handler;

        public MessageDelegateDispatcher(MessageHandler<T> handler)
            : this(handler, 1)
        {
        }

        public MessageDelegateDispatcher(MessageHandler<T> handler, int messagesPerDispatch)
        {
            Handler = handler;
            MessagesPerDispatch = messagesPerDispatch;
        }

        public void Dispatch(Messenger messenger)
        {
            if(Handler == null)
            {
                return;
            }
            if(MessagesPerDispatch == 1)
            {
                T message = default;
                uint messageId = 0;
                if (messenger.Take(ref message, ref messageId))
                {
                    Handler(message, messageId);
                }
            }
            else if(MessagesPerDispatch > 1)
            {
                int actualCount = MessagesPerDispatch;
                var buffer = messenger.Take<T>(ref actualCount);
                for(int i = 0; i < actualCount; i++)
                {
                    Handler(buffer[i].Item1, buffer[i].Item2);
                }
            }
        }
    }
}
