using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.DSLExtension.Modules.SettingTypes;

namespace Wallop.Engine.Scripting
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
            Reload(packageDirectory);
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void Reload(string packageDirectory)
        {
            EngineLog.For<PackageCache>().Info("Recreating package cache with package directory '{packageDir}'...", packageDirectory);


            EngineLog.For<PackageCache>().Debug("Creating TypeCache...");
            Types = new TypeCache();
            EngineLog.For<PackageCache>().Debug("Loading packages...");
            Packages = PackageLoader.LoadPackages(packageDirectory);

            EngineLog.For<PackageCache>().Debug("Resolving modules...");
            Modules = ResolveModules();
        }

        private IEnumerable<Module> ResolveModules()
        {
            EngineLog.For<PackageCache>().Info("Lazily loading modules from {pkgCount} packages...", Packages.Count());

            int moduleCount = 0;
            foreach (var package in Packages)
            {
                foreach (var module in package.DeclaredModules)
                {
                    foreach (var setting in module.ModuleSettings)
                    {
                        setting.CachedType = Types.Types[setting.SettingType];
                    }
                    EngineLog.For<PackageCache>().Info("Resolving module {module}...", module.ModuleInfo);
                    moduleCount++;
                    yield return module;
                }
            }

            EngineLog.For<PackageCache>().Info("Lazy package iteration finished. Loaded {numModules} modules from {numPackage} packages.", moduleCount, Packages.Count());
        }
    }
}
