using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;
using Wallop.DSLExtension.Types.Plugin;
using Wallop.Engine.Scripting.ECS;

namespace Wallop.Engine.Scripting
{
    /*
    internal class ScriptedActorRunner
    {
        private const bool INIT_THREADED = true;
        private const bool UPDATE_THREADED = true;
        private const bool RENDER_THREADED = false;

        public IEnumerable<IScriptEngineProvider> ScriptEngineProviders { get; private set; }

        private List<TaskHandler> _handlers;
        private Dictionary<string, TaskHandler> _handlersByActor;

        public ScriptedActorRunner(IEnumerable<IScriptEngineProvider> scriptEngineProviders)
        {
            ScriptEngineProviders = scriptEngineProviders;

            _handlers = new List<TaskHandler>();
            _handlersByActor = new Dictionary<string, TaskHandler>();
        }

        public void AddActor<TTag>(ScriptedActor actor, TTag tag, Action<IScriptContext> buildContextCallback)
        {
            var engineProvider = ScriptEngineProviders.FirstOrDefault(ep => ep.Name == actor.ControllingModule.ModuleInfo.ScriptEngineId);
            if (engineProvider == null)
            {
                // TODO:
                return;
            }

            var context = engineProvider.CreateContext();
            var engine = engineProvider.CreateScriptEngine(actor.ControllingModule.ModuleInfo.ScriptEngineArgs);
            actor.ScriptEngine = engine;
            engine.AttachContext(context);
            buildContextCallback(context);

            var taskHandler = new TaskHandler(engine);
            taskHandler.Tag = tag;
            _handlersByActor.Add(actor.Name, taskHandler);
            _handlers.Add(taskHandler);
        }

        public void Invoke<TTag>(string action, bool threaded, Action<TTag> before, Action<TTag> after)
        {
            foreach (var handler in _handlers)
            {
                void RunAction(Action a)
                {
                    if (before != null && handler.Tag is TTag beforeTag)
                    {
                        before(beforeTag);
                    }
                    a();
                    if (after != null && handler.Tag is TTag afterTag)
                    {
                        after(afterTag);
                    }
                }

                if (threaded)
                {
                    handler.EnqueueAction<Action>(action, RunAction);
                }
                else
                {
                    handler.RunAction<Action>(action, RunAction);
                }
            }
        }

        public void Invoke(string actorName, string action, bool threaded)
        {
            var handler = _handlersByActor[actorName];
            if (threaded)
            {
                handler.EnqueueAction<Action>(action, a => a());
            }
            else
            {
                handler.EnqueueAction<Action>(action, a => a());
            }
        }

        public void InvokeExecute(string actorName, string source, bool threaded)
        {
            var handler = _handlersByActor[actorName];
            if (threaded)
            {
                handler.EnqueueAction(() => handler.ScriptEngine.Execute(source));
            }
            else
            {
                handler.RunAction(() => handler.ScriptEngine.Execute(source));
            }
        }


        public async Task WaitAsync()
        {
            foreach (var handler in _handlers)
            {
                await handler.WaitForEmptyAsync();
            }
        }
    }
    */
}
