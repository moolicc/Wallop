using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.AutoGen
{
    public enum OptionDefaultValueTimes
    {
        EarlyBound,
        LateBound,
        None,
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class OptionAttribute : Attribute
    {
        public string[] Aliases { get; private set; }

        public string? Description { get; set; }
        public object? DefaultValue { get; set; }
        public Type? ArgumentType { get; set; }
        public OptionDefaultValueTimes DefaultValueBindingType { get; set; }

        public OptionAttribute(params string[] aliases)
        {
            Aliases = aliases;
            DefaultValueBindingType = OptionDefaultValueTimes.LateBound;
        }
    }
}
