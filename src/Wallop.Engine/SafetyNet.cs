using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine
{
    public static class SafetyNet
    {
        public static void Handle<TCaller>(Exception exception)
        {
            // TODO: In the future, we could report this to interested party(/ies).
            EngineLog.For<TCaller>().Error(exception, exception.Message);
        }

        public static Net<TCaller> Handle<TCaller>(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Handle<TCaller>(ex);
                return new Net<TCaller>(ex);
            }

            return new Net<TCaller>();
        }

        public static Net<TCaller> Handle<TCaller, TArg>(Action<TArg> action, TArg argument)
        {
            try
            {
                action(argument);
            }
            catch (Exception ex)
            {
                Handle<TCaller>(ex);
                return new Net<TCaller>(ex);
            }

            return new Net<TCaller>();
        }

        public static Net<TCaller> Handle<TCaller, TArg1, TArg2>(Action<TArg1, TArg2> action, TArg1 argument1, TArg2 argument2)
        {
            try
            {
                action(argument1, argument2);
            }
            catch (Exception ex)
            {
                Handle<TCaller>(ex);
                return new Net<TCaller>(ex);
            }

            return new Net<TCaller>();
        }

        public static Net<TCaller> Handle<TCaller, TArg1, TArg2, TArg3>(Action<TArg1, TArg2, TArg3> action, TArg1 argument1, TArg2 argument2, TArg3 argument3)
        {
            try
            {
                action(argument1, argument2, argument3);
            }
            catch (Exception ex)
            {
                Handle<TCaller>(ex);
                return new Net<TCaller>(ex);
            }

            return new Net<TCaller>();
        }

        public static Net<TCaller> Handle<TCaller, TRet>(Func<TRet> action, out TRet? result)
        {
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                Handle<TCaller>(ex);
                result = default;
                return new Net<TCaller>(ex);
            }

            return new Net<TCaller>(result);
        }


        public static Net<TCaller> Handle<TCaller, TArg, TRet>(Func<TArg, TRet> action, TArg argument, out TRet? result)
        {
            try
            {
                result = action(argument);
            }
            catch (Exception ex)
            {
                Handle<TCaller>(ex);
                result = default;
                return new Net<TCaller>(ex);
            }

            return new Net<TCaller>(result);
        }

        public static Net<TCaller> Handle<TCaller, TArg1, TArg2, TRet>(Func<TArg1, TArg2, TRet> action, TArg1 argument1, TArg2 argument2, out TRet? result)
        {
            try
            {
                result = action(argument1, argument2);
            }
            catch (Exception ex)
            {
                Handle<TCaller>(ex);
                result = default;
                return new Net<TCaller>(ex);
            }

            return new Net<TCaller>(result);
        }

        public static Net<TCaller> Handle<TCaller, TArg1, TArg2, TArg3, TRet>(Func<TArg1, TArg2, TArg3, TRet> action, TArg1 argument1, TArg2 argument2, TArg3 argument3, out TRet? result)
        {
            try
            {
                result = action(argument1, argument2, argument3);
            }
            catch (Exception ex)
            {
                Handle<TCaller>(ex);
                result = default;
                return new Net<TCaller>(ex);
            }

            return new Net<TCaller>(result);
        }
    }
}
