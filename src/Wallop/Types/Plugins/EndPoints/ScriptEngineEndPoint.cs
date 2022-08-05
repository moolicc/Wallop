﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Scripting;
using Wallop.Shared.Types.Plugin;

namespace Wallop.Types.Plugins.EndPoints
{
    internal class ScriptEngineEndPoint : EndPointBase, ILoadingScriptEnginesEndPoint
    {
        internal readonly List<IScriptEngineProvider> EngineProviders;


        public ScriptEngineEndPoint(Messaging.Messenger messages)
            : base(messages)
        {
            EngineProviders = new List<IScriptEngineProvider>();
        }

        public void RegisterScriptEngineProvider<TProvider>(TProvider provider) where TProvider : IScriptEngineProvider
        {
            EngineProviders.Add(provider);
        }

        public IEnumerable<IScriptEngineProvider> GetScriptEngineProviders()
            => EngineProviders.AsReadOnly();

        public IScriptEngineProvider? GetScriptEngineProvider(string providerName)
            => EngineProviders.FirstOrDefault(p => p.Name == providerName);
    }
}
