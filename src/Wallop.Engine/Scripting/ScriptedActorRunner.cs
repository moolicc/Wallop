using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;
using Wallop.Engine.Scripting.ECS;

namespace Wallop.Engine.Scripting
{
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

        public void AddActor(ScriptedActor actor, ScriptContext context)
        {
            var engineProvider = ScriptEngineProviders.FirstOrDefault(ep => ep.Name == actor.ControllingModule.ModuleInfo.ScriptEngineId);
            if(engineProvider == null)
            {
                // TODO:
                return;
            }

            var engine = engineProvider.CreateScriptEngine(actor.ControllingModule.ModuleInfo.ScriptEngineArgs);
            engine.Init(context);

            var taskHandler = new TaskHandler(engine);
            _handlersByActor.Add(actor.Name, taskHandler);
            _handlers.Add(taskHandler);
        }

        public void InvokeInit()
        {
            Invoke("init", INIT_THREADED);
        }

        public void InvokeUpdate()
        {
            Invoke("update", UPDATE_THREADED);
        }

        public void InvokeRender()
        {
            Invoke("draw", RENDER_THREADED);
        }

        public void Invoke(string action, bool threaded)
        {
            foreach (var handler in _handlers)
            {
                if(threaded)
                {
                    handler.EnqueueAction<Action>(action, (a) => a());
                }
                else
                {
                    handler.RunAction<Action>(action, (a) => a());
                }
            }
        }

        public void Invoke(string actorName, string action, bool threaded)
        {
            var handler = _handlersByActor[actorName];
            if(threaded)
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
            if(threaded)
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
}
