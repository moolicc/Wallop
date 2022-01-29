using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine
{
    internal static class Extensions
    {
        public static void WaitAndThrow(this Task task)
        {
            task.Wait();
            if(task.IsFaulted && task.Exception != null)
            {
                throw task.Exception;
            }
        }

        public static T OrThrow<T>(this T? instance)
        {
            if(instance == null)
            {
                throw new ArgumentNullException(typeof(T).Name);
            }
            return instance;
        }

        public static T OrThrow<T>(this T? instance, string exceptionMessage)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(typeof(T).Name, exceptionMessage);
            }
            return instance;
        }

        public static T OrThrow<T, TEx>(this T? instance, string exceptionMessage) where TEx : Exception
        {
            if (instance == null)
            {
                var ex = (TEx?)Activator.CreateInstance(typeof(T), new object[] { exceptionMessage });
                if(ex == null)
                {
                    throw new Exception(exceptionMessage);
                }
                throw ex;
            }
            return instance;
        }
    }
}
