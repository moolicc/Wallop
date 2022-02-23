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

    internal record struct RunTask(Action<object?> Action, object? State);

    public class TaskHandler
    {
        private static int TaskHandlerCount;

        public RunLoopDelays RunLoopDelay { get; set; }
        public bool RunsOnCallingThread { get; private set; }
        public Thread BackingThread => _backingTask;


        private CancellationTokenSource _cancelSource;
        private Thread _backingTask;
        private ConcurrentQueue<RunTask> _taskQueue;


        static TaskHandler()
        {
            TaskHandlerCount = 1;
        }

        public TaskHandler()
            : this(false)
        {
        }

        public TaskHandler(bool runOnCallingThread)
        {
            RunsOnCallingThread = runOnCallingThread;

            RunLoopDelay = RunLoopDelays.ShortDelay;
            _taskQueue = new ConcurrentQueue<RunTask>();

            _cancelSource = new CancellationTokenSource();

            if(runOnCallingThread)
            {
                _backingTask = Thread.CurrentThread;
            }
            else
            {
                _backingTask = new Thread(RunTask);
                _backingTask.IsBackground = true;
                _backingTask.Name = $"TaskHandler # {TaskHandlerCount}";
                _backingTask.Start();
            }

            TaskHandlerCount++;
        }

        public void Terminate()
        {
            _cancelSource.Cancel();
            _taskQueue.Clear();
        }

        public void QueueTask(object? state, Action<object?> action)
        {
            if(RunsOnCallingThread)
            {
                action(state);
                return;
            }

            _taskQueue.Enqueue(new RunTask(action, state));
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
                await Task.Delay(1);
            }
        }

        private void RunTask()
        {
            while (!_cancelSource.IsCancellationRequested)
            {
                if(_taskQueue.TryDequeue(out var scriptTask))
                {
                    scriptTask.Action(scriptTask.State);
                }
            }
        }
    }
}
