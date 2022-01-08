﻿using System.Collections.Generic;
using Wallop.Bridge;

namespace Wallop.Presenter.Scripting
{
    public static class ModuleCache
    {
        private static Dictionary<string, Module> _cache;

        static ModuleCache()
        {
            _cache = new Dictionary<string, Module>();
        }

        public static void LoadModules(string baseDir)
        {
            _cache.Clear();
            foreach (var item in Bridge.Manifest.LoadManifests(baseDir))
            {
                _cache.Add(item.Name, new CsModule(item));
            }
        }

        public static Module GetCachedModuleFromName(string name)
        {
            return _cache[name];
        }
    }
}
