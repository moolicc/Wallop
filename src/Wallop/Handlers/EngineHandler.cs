using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Messaging;
using Wallop.Shared.Messaging.Messages;


namespace Wallop.Handlers
{
    public abstract class EngineHandler
    {
        public EngineApp App { get; private set; }

        internal List<IMessageDispatcher> MessageDispatchers { get; private set; }

        protected EngineHandler(EngineApp app)
        {
            App = app;
            MessageDispatchers = new List<IMessageDispatcher>(10);
        }

        //public void SubscribeToEngineMessages<T>(MessageHandler<T> handler) where T : struct
        //{
        //    MessageDispatchers.Add(new MessageDelegateDispatcher<T>(handler));
        //}

        public void SubscribeToEngineMessages<T>(MessageHandlerReply<T> handler) where T : struct
        {
            MessageDispatchers.Add(new MessageDelegateDispatcher<T>(handler));
        }

        public virtual Command? GetCommandLineCommand(bool firstInstance) { return null; }

        public virtual void BeforeUpdate()
        {
            foreach (var item in MessageDispatchers)
            {
                item.Dispatch(App.Messenger);
            }
        }

        public virtual void AfterUpdate() { }
        public virtual void BeforeDraw() { }
        public virtual void AfterDraw() { }

        public virtual void Shutdown() { }

        protected virtual MessageReply Success(uint messageId, object? content = null)
        {
            string? contentType = null;
            if (content != null)
            {
                contentType = content.GetType().FullName;
            }

            return new MessageReply(messageId, ReplyStatus.Successful, "Operation was successful.", contentType, content);
        }


        protected virtual MessageReply Invalid(uint messageId, string? details = null)
        {
            string contentType = typeof(string).FullName!;
            return new MessageReply(messageId, ReplyStatus.Invalid, "Invalid state.", contentType, details ?? string.Empty);
        }


        protected virtual MessageReply Fail(uint messageId, Exception? exception = null)
        {
            string? contentType = null;
            if (exception != null)
            {
                contentType = exception.GetType().FullName;
            }

            return new MessageReply(messageId, ReplyStatus.Successful, "Operation failed!", contentType, exception);
        }
    }
}
