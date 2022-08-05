using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Scripting;

namespace Wallop.Shared.Types.Plugin
{
    public interface ILoadingScriptEnginesEndPoint
    {
        public abstract void RegisterScriptEngineProvider<TProvider>(TProvider provider) where TProvider : IScriptEngineProvider;
        public abstract IEnumerable<IScriptEngineProvider> GetScriptEngineProviders();
    }
}
