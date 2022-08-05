using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Modules;

namespace PackageGen
{
    public static class Extensions
    {

        // TODO: Display changes in tree.
        public static string ToTreeString(this Package package, ChangeTracking.ChangeSet changeSet)
        {
            var builder = new TreeBuilder();

            var root = builder.SetRoot($"(P) {package.Info.PackageName}");
            root.AddChild($"VER: {package.Info.PackageVersion}");
            root.AddChild($"DESC: {package.Info.PackageDescription}");
            root.AddChild($"PATH: {package.Info.ManifestPath}");

            root.AddChildren(package.Info.PackageVariables.Select(v => $"VAR: {v.Key}={v.Value}"));

            foreach (var module in package.DeclaredModules)
            {
                var node = root.AddChild($"(M) {module.ModuleInfo.ScriptName}");
                node.AddChild($"ID: {module.ModuleInfo.Id}");
                node.AddChild($"VER: {module.ModuleInfo.ScriptVersion}");
                node.AddChild($"DESC: {module.ModuleInfo.ScriptDescription}");
                node.AddChild($"TYPE: {module.ModuleInfo.ScriptType}");
                node.AddChild($"PATH: {module.ModuleInfo.SourcePath}");

                node.AddChildren(module.ModuleInfo.Variables.Select(v => $"VAR: {v.Key}={v.Value}"));
                node.AddChildren(module.ModuleInfo.HostApis.Select(a => $"API: {a}"));

                var engineNode = node.AddChild($"XENGINE: {module.ModuleInfo.ScriptEngineId}");
                foreach (var arg in module.ModuleInfo.ScriptEngineArgs)
                {
                    engineNode.AddChild($"ARG: {arg.Key}={arg.Value}");
                }

                foreach (var setting in module.ModuleSettings)
                {
                    var settingNode = engineNode.AddChild($"(S) {setting.SettingName}");
                    settingNode.AddChild($"DESC: {setting.SettingDescription}");
                    settingNode.AddChild($"TYPE: {setting.SettingType}");
                    settingNode.AddChildren(setting.SettingTypeArgs.Select(sa => $"SETARG: {sa.Key}={sa.Value}"));

                    var req = setting.Required ? "X" : " ";
                    settingNode.AddChild($"REQ: [{req}]");
                    if(!string.IsNullOrEmpty(setting.DefaultValue))
                    {
                        settingNode.AddChild($"DEF: {setting.DefaultValue}");
                    }
                    settingNode.AddChildren(setting.Bindings.Select(b => $"BIND: {b.PropertyName}:{b.TypeName}"));
                }
            }

            return builder.ToString();
        }
    }
}
