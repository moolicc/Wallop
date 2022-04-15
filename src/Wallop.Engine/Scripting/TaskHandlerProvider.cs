using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;

namespace Wallop.Engine.Scripting
{
    public enum ThreadingPolicy
    {
        SingleThread,
        Multithread,
    }

    public enum TaskOperations
    {
        Update,
        Draw,
    }

    public class TaskHandlerProvider
    {
        public ThreadingPolicy UpdatePolicy { get; private set; }
        public ThreadingPolicy DrawPolicy { get; private set; }

        public Action DrawFirstRunCallback;

        private Dictionary<ECS.ScriptedElement, TaskHandler> _updateHandlers;
        private Dictionary<ECS.ScriptedElement, TaskHandler> _drawHandlers;

        private bool _policiesChanged;

        public TaskHandlerProvider(ThreadingPolicy updatePolicy, ThreadingPolicy drawPolicy, Action drawFirstRunCallback)
        {
            UpdatePolicy = updatePolicy;
            DrawPolicy = drawPolicy;
            DrawFirstRunCallback = drawFirstRunCallback;

            _updateHandlers = new Dictionary<ECS.ScriptedElement, TaskHandler>();
            _drawHandlers = new Dictionary<ECS.ScriptedElement, TaskHandler>();
        }

        public void CleanupHandlers(ECS.ScriptedElement element)
        {
            EngineLog.For<TaskHandlerProvider>().Info("Cleaning up handlers for element {element}...", element.Name);

            var handler = _updateHandlers[element];
            _updateHandlers.Remove(element);
            if(UpdatePolicy != ThreadingPolicy.SingleThread)
            {
                EngineLog.For<TaskHandlerProvider>().Info("Terminating update handler on thread {thread}...", handler.BackingThread.Name);
                handler.Terminate();
            }

            handler = _drawHandlers[element];
            _drawHandlers.Remove(element);
            if (DrawPolicy != ThreadingPolicy.SingleThread)
            {
                EngineLog.For<TaskHandlerProvider>().Info("Terminating draw handler on thread {thread}...", handler.BackingThread.Name);
                handler.Terminate();
            }
        }

        internal void SetUpdatePolicy(ThreadingPolicy updateThreadingPolicy)
        {
            UpdatePolicy = updateThreadingPolicy;
            _policiesChanged = true;
        }

        internal void SetDrawPolicy(ThreadingPolicy drawThreadingPolicy)
        {
            DrawPolicy = drawThreadingPolicy;
            _policiesChanged = true;
        }

        public TaskHandler GetUpdateHandler(ECS.ScriptedElement element)
        {
            if (_policiesChanged)
            {
                Recreate();
            }
            if (!_updateHandlers.TryGetValue(element, out var handler))
            {
                if (UpdatePolicy == ThreadingPolicy.SingleThread && _updateHandlers.Count > 0)
                {
                    handler = _updateHandlers.First().Value;
                }
                else
                {
                    handler = new TaskHandler(false);
                    EngineLog.For<TaskHandlerProvider>().Info("Creating update handler for {element} on thread {thread}", element.Name, handler.BackingThread.Name);
                }

                _updateHandlers.Add(element, handler);
            }

            return handler;
        }

        public TaskHandler GetDrawHandler(ECS.ScriptedElement element)
        {
            if(_policiesChanged)
            {
                Recreate();
            }
            if (!_drawHandlers.TryGetValue(element, out var handler))
            {
                if (DrawPolicy == ThreadingPolicy.SingleThread && _drawHandlers.Count > 0)
                {
                    handler = _drawHandlers.First().Value;
                }
                else
                {
                    handler = new TaskHandler(true);
                    EngineLog.For<TaskHandlerProvider>().Info("Creating draw handler for {element} on thread {thread}", element.Name, handler.BackingThread.Name);
                    handler.QueueTask(null, _ => DrawFirstRunCallback());
                }

                _drawHandlers.Add(element, handler);
            }

            return handler;
        }

        private void Recreate()
        {
            foreach (var item in _updateHandlers)
            {
                item.Value.Terminate();
            }
            _updateHandlers.Clear();

            foreach (var item in _drawHandlers)
            {
                item.Value.Terminate();
            }
            _drawHandlers.Clear();

            _policiesChanged = false;
        }
    }
}
