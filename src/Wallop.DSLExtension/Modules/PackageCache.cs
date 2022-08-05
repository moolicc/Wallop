using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Modules;
using Wallop.Shared.Modules.SettingTypes;

namespace Wallop.Shared.Modules
{
    public class PackageCache
    {
        public IEnumerable<Package> Packages { get; private set; }
        public IEnumerable<Module> Modules { get; private set; }

        // TODO: Populate this from plugins.
        public TypeCache Types { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PackageCache(string packageDirectory)
        {
            ReloadAll(packageDirectory);
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void ReloadAll(string packageDirectory)
        {
            Types = new TypeCache();
            Packages = PackageLoader.LoadPackages(packageDirectory).ToArray();
            Modules = ResolveModules();
        }

        public void ReloadPackage(string moduleId)
        {
            // Find and remove the package from the packages list.
            var package = Packages.First(p => p.DeclaredModules.Any(m => m.ModuleInfo.Id == moduleId));
            Packages = Packages.Where(p => p != package);

            // Find and remove the modules that live within that package.
            Modules = Modules.Where(m => !package.DeclaredModules.Any(pm => m.ModuleInfo.Id != pm.ModuleInfo.Id));
        }

        private IEnumerable<Module> ResolveModules()
        {
            int moduleCount = 0;
            foreach (var package in Packages)
            {
                foreach (var module in package.DeclaredModules)
                {
                    bool typeNotFound = false;

                    foreach (var setting in module.ModuleSettings)
                    {
                        if(!Types.Types.ContainsKey(setting.SettingType))
                        {
                            typeNotFound = true;
                            break;
                        }
                        setting.CachedType = Types.Types[setting.SettingType];
                    }
                    if(typeNotFound)
                    {
                        continue;
                    }
                    moduleCount++;
                    yield return module;
                }
            }
        }
    }
}
