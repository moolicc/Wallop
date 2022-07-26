using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Wallop.DSLExtension.Modules
{
    public static class PackageSaver
    {
        public static void SavePackage(Package package, string filepath)
        {
            var root = new XElement("package");

            root.Add(SavePackageMetadata(package));
            root.Add(SaveModules(package.DeclaredModules));

            var document = new XDocument(root);
            document.Save(filepath);
        }

        private static XElement SaveModules(IEnumerable<Module> modules)
        {
            var root = new XElement("modules");

            root.Add(modules.Select(m => SaveModule(m)));

            return root;
        }

        private static XElement SaveModule(Module module)
        {
            var root = new XElement("module");

            root.Add(SaveModuleMetadata(module));
            root.Add(SaveModuleSettings(module));

            return root;
        }

        private static XElement SaveModuleMetadata(Module module)
        {
            var root = new XElement("metadata");
            var apisRoot = new XElement("apis");

            apisRoot.Add(module.ModuleInfo.HostApis.Select(api => new XElement("api", api)));

            root.Add(new XElement("type", module.ModuleInfo.ScriptType.ToString()));
            root.Add(new XElement("source", module.ModuleInfo.SourcePath));
            root.Add(SaveCommonMetadataItems(module.ModuleInfo.ScriptName, module.ModuleInfo.ScriptDescription, module.ModuleInfo.ScriptVersion, module.ModuleInfo.Variables));
            root.Add(SaveModuleScriptEngine(module));
            root.Add(apisRoot);

            return root;
        }

        private static XElement SaveModuleSettings(Module module)
        {
            var root = new XElement("settings");

            root.Add(module.ModuleSettings.Select(s => SaveModuleSetting(s)));

            return root;
        }

        private static XElement SaveModuleSetting(ModuleSetting setting)
        {
            var root = new XElement("setting");

            root.Add(new XElement("name", setting.SettingName));
            root.Add(new XElement("description", setting.SettingDescription));
            root.Add(new XElement("defaultValue", setting.DefaultValue));
            root.Add(new XElement("type", setting.SettingType));
            root.Add(new XElement("required", setting.Required));
            root.Add(new XElement("tracked", setting.Tracked));
            root.Add(SaveSettingBindings(setting));
            root.Add(SaveSettingTypeArgs(setting));

            return root;
        }

        private static XElement SaveModuleScriptEngine(Module module)
        {
            var root = new XElement("scriptEngine");
            var argsRoot = new XElement("args");

            argsRoot.Add(module.ModuleInfo.ScriptEngineArgs.Select(kvp => new XElement(kvp.Key, kvp.Value)));

            root.Add(new XElement("name", module.ModuleInfo.ScriptEngineId));
            root.Add(argsRoot);

            return root;
        }

        private static XElement SaveSettingBindings(ModuleSetting setting)
        {
            var root = new XElement("bindings");

            root.Add(setting.Bindings.Select(b => new XElement(b.TypeName, b.PropertyName)));

            return root;
        }

        private static XElement SaveSettingTypeArgs(ModuleSetting setting)
        {
            var root = new XElement("typeArgs");

            root.Add(setting.SettingTypeArgs.Select(kvp => new XElement(kvp.Key, kvp.Value)));

            return root;
        }

        private static XElement SavePackageMetadata(Package package)
        {
            var root = new XElement("metadata");
            root.Add(SaveCommonMetadataItems(package.Info.PackageName, package.Info.PackageDescription, package.Info.PackageVersion, package.Info.PackageVariables));
            return root;
        }

        private static IEnumerable<XElement> SaveCommonMetadataItems(string name, string description, string version, IEnumerable<KeyValuePair<string, string>> variables)
        {
            XElement variablesElement = new XElement("variables");
            variablesElement.Add(variables.Select(kvp => new XElement(kvp.Key, kvp.Value)));

            return new List<XElement>
            {
                new XElement("name", name),
                new XElement("version", version),
                new XElement("description", description),
                variablesElement
            };
        }
    }
}
