using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Modules
{
    public record PackageInfo(string ManifestPath, string PackageName, string PackageVersion, string PackageDescription, IEnumerable<KeyValuePair<string, string>> PackageVariables);
}
