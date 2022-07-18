using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine
{
    public class Net
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


        public Net Next(Action action)
        {
            if (Success)
            {
                return SafetyNet.Handle(action);
            }
            return this;
        }

        public Net Next<TPrevResult>(Action<TPrevResult> action)
        {
            if (Result != null && Result.GetType() == typeof(TPrevResult))
            {
                return SafetyNet.Handle(action, Result);
            }
            return this;
        }

        public Net Next<TArg1>(Action<TArg1> action, TArg1 argument)
        {
            if (Result != null)
            {
                return SafetyNet.Handle(action, argument);
            }
            return this;
        }





        public Net Next<TRet>(Func<TRet> action)
        {
            if (Success)
            {
                return SafetyNet.Handle(action);
            }
            return this;
        }

        public Net Next<TPrevResult, TRet>(Func<TPrevResult, TRet> action)
        {
            if (Result != null && Result.GetType() == typeof(TPrevResult))
            {
                return SafetyNet.Handle(action, Result);
            }
            return this;
        }

        public Net Next<TArg1, TRet>(Func<TArg1, TRet> action, TArg1 argument)
        {
            if (Result != null)
            {
                return SafetyNet.Handle(action, argument);
            }
            return this;
        }



        public Net Then(Action action)
        {
            if(Success)
            {
                action();
            }
            return this;
        }

        public Net Then<TResult>(Action<TResult> action)
        {
            if(Result != null && Result.GetType() == typeof(TResult))
            {
                action((TResult)Result);
            }
            return this;
        }

        public Net Catch(Action<Exception> action)
        {
            if(Exception != null)
            {
                action(Exception);
            }
            return this;
        }
    }
}
