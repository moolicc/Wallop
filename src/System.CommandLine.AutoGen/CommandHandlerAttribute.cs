using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.AutoGen
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class CommandHandlerAttribute : Attribute
    {
        public CommandHandlerAttribute()
        {
        }
    }
}
