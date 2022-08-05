using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Scripting
{
    public enum FactoryPropertyMethod
    {
        Getter,
        Setter,
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class ScriptPropertyFactoryAttribute : Attribute
    {
        public FactoryPropertyMethod Accessor { get; private set; }
        public string PropertyId { get; private set; }

        public string? ExposedName { get; set; }


        public ScriptPropertyFactoryAttribute(string propertyId, FactoryPropertyMethod designation)
        {
            PropertyId = propertyId;
            Accessor = designation;
            ExposedName = null;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class ScriptPropertyAttribute : Attribute
    {
        public string? ExposedName { get; set; }
        public bool? Readable { get; set; }
        public bool? Writable { get; set; }


        public ScriptPropertyAttribute()
        {
            ExposedName = null;
            Readable = null;
            Writable = null;
        }
    }
}
