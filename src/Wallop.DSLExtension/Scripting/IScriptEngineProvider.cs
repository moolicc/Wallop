using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Scripting
{
    public interface IScriptEngineProvider
    {
        public string Name { get; }

        public IScriptEngine CreateScriptEngine(IEnumerable<KeyValuePair<string, string>> engineArgs);
    }
}
