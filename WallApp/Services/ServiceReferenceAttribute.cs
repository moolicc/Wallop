using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.Services
{
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    sealed class ServiceReferenceAttribute : Attribute
    {
        public string Key { get; set; }
    }
}
