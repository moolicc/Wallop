using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Messaging.Messages;

namespace Wallop.Messaging
{
    public delegate void MessageHandler<T>(T message, uint messageId) where T : struct;
    public delegate object? MessageHandlerReply<T>(T message, uint messageId) where T : struct;

    public class MessageDelegateDispatcher<T> : IMessageDispatcher
        where T : struct
    {
        public int MessagesPerDispatch { get; set; }
        public MessageHandler<T>? Handler;
        public MessageHandlerReply<T>? HandlerWithReply;

        public MessageDelegateDispatcher(MessageHandler<T> handler)
            : this(handler, null, 1)
        {
        }

        public MessageDelegateDispatcher(MessageHandler<T> handler, int messagesPerDispatch)
            : this(handler, null, messagesPerDispatch)
        {
        }

        public MessageDelegateDispatcher(MessageHandlerReply<T> handler)
            : this(null, handler, 1)
        {
        }

        public MessageDelegateDispatcher(MessageHandlerReply<T> handler, int messagesPerDispatch)
            : this(null, handler, messagesPerDispatch)
        {
        }

        private MessageDelegateDispatcher(MessageHandler<T>? handler, MessageHandlerReply<T>? handlerWithReply, int messagesPerDispatch)
        {
            MessagesPerDispatch = messagesPerDispatch;
            Handler = handler;
            HandlerWithReply = handlerWithReply;
        }

        public void Dispatch(Messenger messenger)
        {
            if(Handler == null && HandlerWithReply == null)
            {
                return;
            }
            if(MessagesPerDispatch == 1)
            {
                T message = default;
                uint messageId = 0;
                if (messenger.Take(ref message, ref messageId))
                {
                    Handle(messenger, message, messageId);
                }
            }
            else if(MessagesPerDispatch > 1)
            {
                int actualCount = MessagesPerDispatch;
                var buffer = messenger.Take<T>(ref actualCount);
                for(int i = 0; i < actualCount; i++)
                {
                    Handle(messenger, buffer[i].Payload, buffer[i].MessageId);
                }
            }
        }

        private void Handle(Messenger messenger, T message, uint messageId)
        {
            if (Handler != null)
            {
                Handler(message, messageId);
            }
            else if (HandlerWithReply != null)
            {
                var replyContent = HandlerWithReply(message, messageId);

                if(replyContent == null)
                {
                    return;
                }

                if (replyContent is not MessageReply)
                {
                    replyContent = new MessageReply(messageId, ReplyStatus.NotSpecified, "", replyContent.GetType(), replyContent);
                }

                messenger.Put(replyContent, typeof(MessageReply));
            }
        }
    }
}
