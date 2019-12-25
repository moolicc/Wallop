using System.Collections.Generic;
using WallApp.Bridge;

namespace WallApp.Engine.Scripting
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
            foreach (var item in Resolver.LoadManifests(baseDir))
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
