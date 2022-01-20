using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Types.Plugin
{
    public interface ILoadingScriptEnginesEndPoint
    {
        public abstract void RegisterScriptEngineProvider<TProvider>(TProvider provider) where TProvider : Scripting.IScriptEngineProvider;
        public abstract IEnumerable<Scripting.IScriptEngineProvider> GetScriptEngineProviders();
    }
}
