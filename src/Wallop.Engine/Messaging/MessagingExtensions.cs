using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.Messaging.Messages;

namespace Wallop.Engine.Messaging
{
    public static class MessagingExtensions
    {
        public static void Reply<T>(this Messenger messenger, uint messageId, T data)
        {
            var reply = new MessageReply(messageId, typeof(T), data);
            messenger.Put(reply);
        }

        public static bool TryGetReply<T>(this Messenger messenger, uint messageId, out T? reply)
        {
            // Hold the reply at the time of the queue.
            uint incomingReplyId = 0;
            MessageReply incomingReply = new MessageReply();

            // Seed our first reply.
            if (!messenger.Take(ref incomingReply, ref incomingReplyId))
            {
                reply = default(T);
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
                if(incomingReplyId == startingIncomingId)
                {
                    result = false;
                    break;
                }
            }

            // If we failed somewhere in the loop, return as such.
            if(!result)
            {
                reply = default(T);
                return false;
            }

            // Return the data cast to the correct Type.
            if(incomingReply.Data is null)
            {
                reply = default(T);
            }
            else
            {
                reply = (T)incomingReply.Data;
            }
            return true;
        }

        public static T? AwaitReply<T>(this Messenger messenger, uint messageId)
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
            if (incomingReply.Data is not null)
            {
                return (T)incomingReply.Data;
            }
            return default(T);
        }
    }
}
