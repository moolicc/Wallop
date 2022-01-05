using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginPantry
{
    internal static class EndPointRunner
    {
        public static Dictionary<string, CancellationTokenSource> CancelTokens { get; private set; }

        static EndPointRunner()
        {
            CancelTokens = new Dictionary<string, CancellationTokenSource>();
        }
    }

    internal static class EndPointRunner<T>
    {
        private static long _runningExecutionTicks;
        private static long _executions;

        public static async Task InvokeEndPointAsync(T? endPointInstance)
        {
            var args = new Dictionary<string, object?>();
            if(endPointInstance != null)
            {
                foreach (var property in endPointInstance.GetType().GetProperties())
                {
                    args.Add(property.Name, property?.GetValue(endPointInstance));
                }
            }
            await InvokeEndPointAsync(args);
        }

        public static async Task InvokeEndPointAsync(Dictionary<string, object?> args)
        {
            await Task.Run(() => EndPointTable<T>.VisitEntries(endPoint =>
            {
                if (endPoint.ExecutionTask == null)
                {
                    endPoint.ExecutionTask = Task.Factory.StartNew(() =>
                    {
                        endPoint.ExecutionStartTime = DateTime.Now.Ticks;
                        InvokeEndPoint(endPoint, args);
                        endPoint.ExecutionEndTime = DateTime.Now.Ticks;
                        _executions++;
                        _runningExecutionTicks += endPoint.ExecutionEndTime - endPoint.ExecutionStartTime;
                    }, GetCancellationToken(endPoint));
                    return;
                }

                if (endPoint.ExecutionTask.Exception != null)
                {
                    // TODO: Bubble this up.
                    Console.WriteLine("Plugin endpoint has had an exception! EndPoint: {0}", endPoint.Name);
                    Console.WriteLine(endPoint.ExecutionTask.Exception.ToString());
                }

                if (endPoint.ExecutionTask.IsCompleted)
                {
                    endPoint.ExecutionTask = Task.Factory.StartNew(() =>
                    {
                        endPoint.ExecutionStartTime = DateTime.Now.Ticks;
                        InvokeEndPoint(endPoint, args);
                        endPoint.ExecutionEndTime = DateTime.Now.Ticks;
                        _executions++;
                        _runningExecutionTicks += endPoint.ExecutionEndTime - endPoint.ExecutionStartTime;
                    }, GetCancellationToken(endPoint));
                }
                else if (DateTime.Now.Ticks - endPoint.ExecutionStartTime > (long)(GetAverageExecutionTicks() * 1.5f))
                {
                    // TODO: Bubble this up.
                    Console.WriteLine("Plugin endpoint is stalling! EndPoint: {0}", endPoint.Name);
                }
            }));
        }

        private static CancellationToken GetCancellationToken(EndPointTableEntry endPoint)
        {
            CancellationToken token;
            if(endPoint.ExecutionTaskCancelToken != null)
            {
                token = endPoint.ExecutionTaskCancelToken.Token;
            }
            if(EndPointRunner.CancelTokens.TryGetValue(endPoint.PluginId, out var tokenSource))
            {
                token = tokenSource.Token;
            }
            else
            {
                var source = new CancellationTokenSource();
                EndPointRunner.CancelTokens.Add(endPoint.PluginId, source);
                token = source.Token;
            }
            return token;
        }

        private static void InvokeEndPoint(EndPointTableEntry endPoint, Dictionary<string, object?> args)
        {
            var passedArgs = new List<object>();

            bool signatureFound = true;

            foreach (var param in endPoint.Target.GetParameters())
            {
                object? value = null;
                if(!args.TryGetValue(param.Name ?? "", out value))
                {
                    bool found = false;
                    foreach (var arg in args)
                    {
                        if(arg.Value?.GetType() == param.ParameterType)
                        {
                            value = arg.Value;
                            found = true;
                            break;
                        }
                    }
                    if(!found)
                    {
                        if(param.IsOptional)
                        {
                            value = param.DefaultValue;
                        }
                        else
                        {
                            signatureFound = false;
                            break;
                        }
                    }
                }
            }

            if(!signatureFound)
            {
                Console.WriteLine("Plugin endpoint is invalid! EndPoint: {0}", endPoint.Name);
            }

            endPoint.Target.Invoke(endPoint.Instance, passedArgs.ToArray());
        }

        private static long GetAverageExecutionTicks()
        {
            long ticks = _runningExecutionTicks;
            long executions = _executions;
            return ticks / executions;
        }
    }
}
