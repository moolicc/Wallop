using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Scripting
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class ScriptFunctionFactoryAttribute : Attribute
    {
        public string? ExposedName { get; set; }


        public ScriptFunctionFactoryAttribute()
        {
            ExposedName = null;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class ScriptFunctionAttribute : Attribute
    {
        public string? ExposedName { get; set; }
        public Type BackingDelegateType { get; private set; }


        public ScriptFunctionAttribute(Type backingDelegateType)
        {
            BackingDelegateType = backingDelegateType;
            ExposedName = null;
        }
    }
}
