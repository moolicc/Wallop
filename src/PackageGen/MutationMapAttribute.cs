using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageGen
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class MutationMapAttribute : Attribute
    {
        public string MapTo { get; }
        public Type DelegateType { get; }

        public MutationMapAttribute(string mapTo, Type delegateType)
        {
            MapTo = mapTo;
            DelegateType = delegateType;    
        }
    }
}
