using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Modules;

namespace PackageGen
{
    public static class IDHelper
    {
        private static Dictionary<int, Package> _packages;
        private static Dictionary<int, Module> _modules;

        private static int _lastPkgId;
        private static int _lastModId;

        static IDHelper()
        {
            _packages = new Dictionary<int, Package>();
            _modules = new Dictionary<int, Module>();
            _lastPkgId = 0;
            _lastModId = 0;
        }

        public static int Register(Package package)
        {
            _lastPkgId++;
            _packages.Add(_lastPkgId, package);
            return _lastPkgId;
        }

        public static int Register(Module module)
        {
            _lastModId++;
            _modules.Add(_lastModId, module);
            return _lastModId;
        }

        public static Package GetPackageById(int id)
            => _packages[id];

        public static Module GetModuleById(int id)
            => _modules[id];


        public static int FindPackageId(Package package)
        {
            foreach (var item in _packages)
            {
                if(item.Value == package)
                {
                    return item.Key;
                }
            }
            return -1;
        }

        public static int FindModuleId(Module module)
        {
            foreach (var item in _modules)
            {
                if (item.Value == module)
                {
                    return item.Key;
                }
            }
            return -1;
        }
    }
}
