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

        public static string Dump(this object? instance)
        {
            if (instance is null)
            {
                return "NULL";
            }
            else if (instance is string str)
            {
                return str;
            }
            else if (instance is bool bl)
            {
                return bl.ToString();
            }
            else if (instance is byte b)
            {
                return b.ToString();
            }
            else if (instance is short s)
            {
                return s.ToString();
            }
            else if (instance is ushort us)
            {
                return us.ToString();
            }
            else if (instance is int i)
            {
                return i.ToString();
            }
            else if (instance is uint ui)
            {
                return ui.ToString();
            }
            else if (instance is long l)
            {
                return l.ToString();
            }
            else if (instance is ulong ul)
            {
                return ul.ToString();
            }
            else if (instance is float f)
            {
                return f.ToString();
            }
            else if (instance is double d)
            {
                return d.ToString();
            }
            else if (instance is decimal dl)
            {
                return dl.ToString();
            }

            var builder = new StringBuilder();
            var type = instance.GetType();
            builder.AppendFormat("[{0}: ", type.Name);
            if(instance == null)
            {
                builder.Append("NULL");
            }
            else
            {
                builder.Append("{ ");

                var properties = type.GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    var item = properties[i];
                    if(i != 0)
                    {
                        builder.Append(", ");
                    }

                    builder.AppendFormat("{0}: {1}", item.Name, item.GetValue(instance).Dump());
                }

                builder.Append(" }");
            }


            builder.Append("]");
            return builder.ToString();
        }
    }
}
