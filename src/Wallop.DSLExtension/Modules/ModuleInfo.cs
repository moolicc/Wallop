using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Modules
{
    public enum ModuleTypes
    {
        Director,
        Actor,
    }

    // TODO: ModuleSettings
    public record ModuleInfo(string PackageFile, string BaseDirectory, string SourcePath, string ScriptName, string ScriptVersion, string ScriptDescription, string ScriptEngineId, IEnumerable<KeyValuePair<string, string>> ScriptEngineArgs, ModuleTypes ScriptType, IEnumerable<string> HostApis, IEnumerable<KeyValuePair<string, string>> Variables)
    {
        public string Id => $"{ScriptName.Replace(' ', '.')}{ScriptVersion}";
    }
}
