using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.Messaging;

namespace Wallop.Engine.Handlers
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

        public void SubscribeToEngineMessages<T>(MessageHandler<T> handler) where T : struct
        {
            MessageDispatchers.Add(new MessageDelegateDispatcher<T>(handler));
        }

        public virtual Command? GetCommandLineCommand() { return null; }

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
    }
}
