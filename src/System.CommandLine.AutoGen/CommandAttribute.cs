using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.AutoGen
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        public string? Description { get; set; }
        public string Name { get; private set; }


        public CommandAttribute(string name)
        {
            Name = name;
        }
    }
}