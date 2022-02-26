using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.AutoGen
{
    public class AutoGenCommandHandler : ICommandHandler
    {
        private Dictionary<string, PropertyInfo> _propertyBindings;

        public Task<int> InvokeAsync(InvocationContext context)
        {
            foreach (var result in context.ParseResult.CommandResult.Children)
            {
                if(result is Parsing.OptionResult option)
                {
                    PropertyInfo? property = null;
                    foreach (var alias in option.Option.Aliases)
                    {
                        if(_propertyBindings.TryGetValue(alias, out var propertyBinding))
                        {
                        }
                    }
                }
            }

            return Task.FromResult(0);
        }
    }
}
