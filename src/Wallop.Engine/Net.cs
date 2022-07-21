using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine
{
    public class Net<TCaller>
    {
        public Exception? Exception { get; private set; }
        public bool Success { get; private set; }
        public object? Result { get; private set; }

        public Net(Exception ex)
        {
            Exception = ex;
            Success = false;
            Result = null;
        }

        public Net(object? result)
        {
            Exception = null;
            Success = true;
            Result = result;
        }

        public Net()
        {
            Exception = null;
            Success = true;
            Result = null;
        }


        public Net<TCaller> Next(Action action)
        {
            if (Success)
            {
                return SafetyNet.Handle<TCaller>(action);
            }
            return this;
        }

        public Net<TCaller> Next<TPrevResult>(Action<TPrevResult> action)
        {
            if (Result != null && Result.GetType() is TPrevResult result)
            {
                return SafetyNet.Handle<TCaller, TPrevResult>(action, result);
            }
            return this;
        }

        public Net<TCaller> Next<TArg1>(Action<TArg1> action, TArg1 argument)
        {
            if (Success)
            {
                return SafetyNet.Handle<TCaller, TArg1>(action, argument);
            }
            return this;
        }





        public Net<TCaller> Next<TRet>(Func<TRet> action)
        {
            if (Success)
            {
                return SafetyNet.Handle<TCaller, TRet>(action, out _);
            }
            return this;
        }

        public Net<TCaller> Next<TPrevResult, TRet>(Func<TPrevResult, TRet> action)
        {
            if (Result != null && Result.GetType() is TPrevResult result)
            {
                return SafetyNet.Handle<TCaller, TPrevResult, TRet>(action, result, out _);
            }
            return this;
        }

        public Net<TCaller> Next<TArg1, TRet>(Func<TArg1, TRet> action, TArg1 argument)
        {
            if (Result != null)
            {
                return SafetyNet.Handle<TCaller, TArg1, TRet>(action, argument, out _);
            }
            return this;
        }



        public Net<TCaller> Then(Action action)
        {
            if(Success)
            {
                action();
            }
            return this;
        }

        public Net<TCaller> Then<TResult>(Action<TResult> action)
        {
            if(Result != null && Result.GetType() == typeof(TResult))
            {
                action((TResult)Result);
            }
            return this;
        }

        public Net<TCaller> Catch(Action<Exception> action)
        {
            if(Exception != null)
            {
                action(Exception);
            }
            return this;
        }
    }
}
