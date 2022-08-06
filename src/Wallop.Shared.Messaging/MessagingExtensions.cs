using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Messaging.Messages;

namespace Wallop.Shared.Messaging
{
    public static class MessagingExtensions
    {
        public static void ReplySuccess(this IMessenger messenger, uint messageId, object? replyPayload = null)
        {
            string? payloadType = null;
            if (replyPayload != null)
            {
                payloadType = replyPayload.GetType().FullName;
            }

            var reply = new MessageReply(messageId, ReplyStatus.Successful, "Operation successful.", payloadType, replyPayload);
            messenger.Put(reply);
        }

        public static void ReplyInvalid(this IMessenger messenger, uint messageId, string details)
        {
            var reply = new MessageReply(messageId, ReplyStatus.Failed, "Operation invalid.", typeof(string).FullName, details);
            messenger.Put(reply);
        }

        public static void ReplyFailed(this IMessenger messenger, uint messageId, string? statusMessage = null, Exception? errorPayload = null)
        {
            string? payloadType = null;
            if (errorPayload != null)
            {
                payloadType = errorPayload.GetType().FullName;
            }
            var reply = new MessageReply(messageId, ReplyStatus.Failed, statusMessage ?? "Operation failed.", payloadType, errorPayload);
            messenger.Put(reply);
        }

        public static void Reply<T>(this IMessenger messenger, uint messageId, ReplyStatus status, string statusMessage, T data)
        {
            var reply = new MessageReply(messageId, status, statusMessage, typeof(T).FullName, data);
            messenger.Put(reply);
        }

        public static void Reply<T>(this IMessenger messenger, MessageReply reply)
        {
            messenger.Put(reply);
        }

        public static bool TryGetReply<T>(this IMessenger messenger, uint messageId, out T? reply)
        {
            // Hold the reply at the time of the queue.
            uint incomingReplyId = 0;
            MessageReply incomingReply = new MessageReply();

            // Seed our first reply.
            if (!messenger.Take(ref incomingReply, ref incomingReplyId))
            {
                reply = default;
                return false;
            }

            // Hold the first ID we check.
            uint startingIncomingId = incomingReplyId;

            // Loop until the messenger.Take fails, we wrap back around the queue to the first ID we checked, or we find
            // the reply for our message ID.
            var result = true;
            while (incomingReplyId != messageId)
            {
                if (!messenger.Take(ref incomingReply, ref incomingReplyId))
                {
                    result = false;
                    break;
                }

                messenger.Put(incomingReply, incomingReplyId);

                // If we've wrapped back around the queue to the start, forget it.
                if (incomingReplyId == startingIncomingId)
                {
                    result = false;
                    break;
                }
            }

            // If we failed somewhere in the loop, return as such.
            if (!result)
            {
                reply = default;
                return false;
            }

            // Return the data cast to the correct Type.
            if (incomingReply.Content is null)
            {
                reply = default;
            }
            else
            {
                reply = (T)incomingReply.Content;
            }
            return true;
        }

        public static T? AwaitReply<T>(this IMessenger messenger, uint messageId)
        {
            // Hold the reply at the time of the queue.
            uint incomingReplyId = 0;
            MessageReply incomingReply = new MessageReply();

            // Seed our first reply.
            while (!messenger.Take(ref incomingReply, ref incomingReplyId))
            {
            }

            // Loop until the we find the reply for our message ID.
            while (incomingReplyId != messageId)
            {
                if (!messenger.Take(ref incomingReply, ref incomingReplyId))
                {
                    Thread.Sleep(5);
                    continue;
                }

                messenger.Put(incomingReply, incomingReplyId);
            }

            // Return the data cast to the correct Type.
            if (incomingReply.Content is not null)
            {
                return (T)incomingReply.Content;
            }
            return default;
        }
    }
}
