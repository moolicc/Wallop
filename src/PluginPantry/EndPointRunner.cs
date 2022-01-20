using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry
{
    internal class PluginTaskContext
    {
        private const int TASK_DELAY_TIME = 100;
        private const int LONG_DELAY = 1000;
        private const int MAX_FAILS_BEFORE_LONG_DELAY = 3000 / TASK_DELAY_TIME;

        public ConcurrentQueue<Action> RunQueue { get; private set; }
        public CancellationTokenSource CancelSource { get; private set; }
        public Task Task { get; private set; }

        public PluginTaskContext()
        {
            RunQueue = new ConcurrentQueue<Action>();
            CancelSource = new CancellationTokenSource();
            Task = PluginTaskAsync();
        }

        private async Task PluginTaskAsync()
        {
            int consecutiveFailedIterations = 0;
            while (!CancelSource.IsCancellationRequested)
            {
                if (RunQueue.TryDequeue(out var nextRun))
                {
                    consecutiveFailedIterations = 0;
                    nextRun();
                }
                else
                {
                    consecutiveFailedIterations++;
                }

                if(consecutiveFailedIterations >= MAX_FAILS_BEFORE_LONG_DELAY)
                {
                    consecutiveFailedIterations = 0;
                    await Task.Delay(LONG_DELAY);
                }
                else
                {
                    await Task.Delay(TASK_DELAY_TIME);
                }
            }
        }
    }

    internal static class EndPointRunner
    {
        private const int DELAY_FOR_TASK_JOIN = 500;
        private static Dictionary<string, PluginTaskContext> _taskContexts;

        static EndPointRunner()
        {
            _taskContexts = new Dictionary<string, PluginTaskContext>();
        }

        public static void StopPluginTasks(string pluginId)
        {
            if(_taskContexts.TryGetValue(pluginId, out var taskContext))
            {
                taskContext.CancelSource.Cancel();
                taskContext.Task.Wait(DELAY_FOR_TASK_JOIN);
                taskContext.RunQueue.Clear();
                taskContext.Task.Dispose();
                taskContext.CancelSource.Dispose();
                _taskContexts.Remove(pluginId);
            }
            else
            {
                throw new KeyNotFoundException(pluginId);
            }
        }

        public static void QueueRun(string pluginId, Action run)
        {
            if(_taskContexts.TryGetValue(pluginId, out var context))
            {
                context.RunQueue.Enqueue(run);
            }
            else
            {
                context = new PluginTaskContext();
                context.RunQueue.Enqueue(run);
                _taskContexts.Add(pluginId, context);
            }
        }


    }


    internal class EndPointRunner<TEndPointContext>
    {
        private static Dictionary<PluginContext, EndPointRunner<TEndPointContext>> _instances;


        public bool LastInvocationCompleted
        {
            get
            {
                return _completeFromLastInvocation == _expectedInvocationCount;
            }
        }

        private PluginContext _pluginContext;
        private long _runningExecutionTicks;
        private long _executions;


        private int _curInvocation;
        private int _completeFromLastInvocation;
        private int _expectedInvocationCount;

        static EndPointRunner()
        {
            _instances = new Dictionary<PluginContext, EndPointRunner<TEndPointContext>>();
        }

        private EndPointRunner(PluginContext context)
        {
            _pluginContext = context;
            _curInvocation = 0;
            _completeFromLastInvocation = 0;
            _expectedInvocationCount = 0;
        }

        public static EndPointRunner<TEndPointContext> ForPluginContext(PluginContext context)
        {
            if (!_instances.TryGetValue(context, out var endPointRunner))
            {
                endPointRunner = new EndPointRunner<TEndPointContext>(context);
                _instances.Add(context, endPointRunner);
            }
            return endPointRunner;
        }

        public void InvokeEndPoint(Func<TEndPointContext?> contextCreator)
        {
            _completeFromLastInvocation = 0;
            _curInvocation++;
            _expectedInvocationCount = EndPointTable<TEndPointContext>.ForPluginContext(_pluginContext).GetEntryCount();
            EndPointTable<TEndPointContext>.ForPluginContext(_pluginContext).VisitEntries(endPoint =>
            {
                EndPointRunner.QueueRun(endPoint.PluginId, () =>
                {
                    int myInvocation = _curInvocation;

                    endPoint.ExecutionStartTime = DateTime.Now.Ticks;
                    InvokeEndPoint(endPoint, contextCreator());
                    endPoint.ExecutionEndTime = DateTime.Now.Ticks;

                    _executions++;
                    _runningExecutionTicks += endPoint.ExecutionEndTime - endPoint.ExecutionStartTime;

                    // If another invocation has been called before this one finished.
                    if(myInvocation == _curInvocation)
                    {
                        _completeFromLastInvocation++;
                    }
                });

                if (DateTime.Now.Ticks - endPoint.ExecutionStartTime > (long)(GetAverageExecutionTicks() * 1.5f))
                {
                    // TODO: Bubble this up.
                }
            });
        }

        private void InvokeEndPoint(EndPointTableEntry endPoint, TEndPointContext? context)
        {
            var invocationResult = Util.TryInvokeMatchingMethod(endPoint.Target, endPoint.Instance, context);
            if(invocationResult == MethodInvocationResults.Failed)
            {
                 // TODO
            }
            else if(invocationResult == MethodInvocationResults.ExpectedStaticMethod)
            {
                // TODO
            }
        }

        private long GetAverageExecutionTicks()
        {
            if (_executions == 0)
            {
                return 0;
            }
            long ticks = _runningExecutionTicks;
            long executions = _executions;
            return ticks / executions;
        }
    }
}
