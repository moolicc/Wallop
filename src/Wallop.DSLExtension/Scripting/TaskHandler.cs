using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Scripting
{
    public enum RunLoopDelays
    {
        ShortDelay,
        LongDelay,
    }

    public class TaskHandler
    {
        private const int SHORT_DELAY = 100;
        private const int LONG_DELAY = 1000;

        public IScriptEngine ScriptEngine { get; private set; }
        public RunLoopDelays RunLoopDelay { get; set; }
        public object? Tag { get; set; }

        private CancellationTokenSource _cancelSource;
        private Task _scriptTask;
        private ConcurrentQueue<Action> _taskQueue;

        public TaskHandler(IScriptEngine scriptEngine)
        {
            ScriptEngine = scriptEngine;
            RunLoopDelay = RunLoopDelays.ShortDelay;
            _taskQueue = new ConcurrentQueue<Action>();

            _cancelSource = new CancellationTokenSource();
            _scriptTask = Task.Factory.StartNew(RunScript, _cancelSource.Token);
        }

        public void EnqueueAction<TScriptAction>(string scriptTask, Action<TScriptAction> onExecute)
        {
            _taskQueue.Enqueue(() => onExecute(ScriptEngine.GetAttachedScriptContext().GetDelegateAs<TScriptAction>(scriptTask)));
        }

        public void EnqueueAction(Action task)
        {
            _taskQueue.Enqueue(task);
        }

        public void RunAction<TScriptAction>(string scriptTask, Action<TScriptAction> onExecute)
        {
            onExecute(ScriptEngine.GetAttachedScriptContext().GetDelegateAs<TScriptAction>(scriptTask));
        }

        public void RunAction(Action task)
        {
            task();
        }

        public bool IsQueueEmpty()
            => _taskQueue.Count == 0;

        public async Task WaitForEmptyAsync()
        {
            if(_taskQueue.Count == 0)
            {
                return;
            }

            while (_taskQueue.Count > 0)
            {
                await Task.Delay(100);
            }
        }

        private async Task RunScript()
        {
            while (!_cancelSource.IsCancellationRequested)
            {
                if(_taskQueue.TryDequeue(out var scriptTask))
                {
                    scriptTask();
                }

                var delayMs = SHORT_DELAY;
                if(RunLoopDelay == RunLoopDelays.LongDelay)
                {
                    delayMs = LONG_DELAY;
                }
                await Task.Delay(delayMs, _cancelSource.Token);
            }
        }
    }
}
