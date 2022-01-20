using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Wallop.DSLExtension.Modules
{
    public static class PackageLoader
    {
        public static IEnumerable<Package> LoadPackages(string directory)
        {
            foreach (var filePath in Directory.GetFiles(directory, "package.xml", SearchOption.AllDirectories))
            {
                yield return LoadPackage(filePath);
            }
        }

        private static Package LoadPackage(string file)
        {
            var document = XDocument.Load(file);
            var root = document.Root;
            if(root == null || root.Name != "package")
            {
                throw new XmlException("Failed to find root of document.");
            }
            var packageInfo = LoadPackageInfo(file, root.XPathSelectElement("./metadata"));
            var modules = LoadModules(root.XPathSelectElement("./modules"));

            return new Package()
            {
                Info = packageInfo,
                DeclaredModules = modules
            };
        }

        private static PackageInfo LoadPackageInfo(string xmlFile, XElement? rootElement)
        {
            if (rootElement == null)
            {
                throw new XmlException("Failed to find package metadata.");
            }
            var commonData = GetCommonMetadataValues(rootElement);
            return new PackageInfo(xmlFile, commonData.Name, commonData.Version, commonData.Description);
        }

        private static IEnumerable<Module> LoadModules(XElement? rootElement)
        {
            if (rootElement == null)
            {
                throw new XmlException("Failed to find package module node.");
            }
            foreach (var element in rootElement.XPathSelectElements("./module"))
            {
                yield return LoadModule(element);
            }
        }

        private static Module LoadModule(XElement? moduleElement)
        {
            if (moduleElement == null)
            {
                throw new XmlException("Failed to load module from package.");
            }

            var info = LoadModuleInfo(moduleElement.XPathSelectElement("./metadata"));
            var settings = LoadModuleSettings(moduleElement.XPathSelectElement("./settings"));
            return new Module()
            {
                ModuleInfo = info,
                ModuleSettings = settings
            };
        }

        private static ModuleInfo LoadModuleInfo(XElement? metadataElement)
        {
            if(metadataElement == null)
            {
                throw new XmlException("Failed to load module metadata.");
            }
            var engineInfo = GetModuleScriptEngineInfo(metadataElement);
            var commonMetadata = GetCommonMetadataValues(metadataElement);
            var moduleType = metadataElement?.XPathSelectElement("./type")?.Value;
            var moduleSourceFile = metadataElement?.XPathSelectElement("./source")?.Value;

            if (moduleType == null || !Enum.TryParse<ModuleTypes>(moduleType, true, out var typeEnumValue))
            {
                throw new XmlException("Module metadata does not contain a valid type node.");
            }
            if(moduleSourceFile == null)
            {
                throw new XmlException("Module metadata does not contain a source file node.");
            }
            else if(!File.Exists(moduleSourceFile))
            {
                // What if potentially a module has its source created dynamically?
                // Well I guess that's an edge-case that has to be dealt with on a case-by-case basis.
                throw new XmlException("Module source file not found.");
            }

            return new ModuleInfo(moduleSourceFile, commonMetadata.Name, commonMetadata.Version, commonMetadata.Description, engineInfo.ScriptEngineName, engineInfo.EngineArgs, typeEnumValue);
        }

        private static IEnumerable<ModuleSetting> LoadModuleSettings(XElement? settingRoot)
        {
            if (settingRoot == null)
            {
                yield break;
            }

            foreach (var settingElement in settingRoot.XPathSelectElements("./setting"))
            {
                var setting = LoadModuleSetting(settingElement);
                if(setting != null)
                {
                    yield return setting;
                }
            }
        }

        private static ModuleSetting? LoadModuleSetting(XElement? settingElement)
        {
            if (settingElement == null)
            {
                return null;
            }

            var name = settingElement.XPathSelectElement("name")?.Value;
            var description = settingElement.XPathSelectElement("description")?.Value ?? "";
            var type = settingElement.XPathSelectElement("type")?.Value;
            var required = settingElement.XPathSelectElement("required")?.Value ?? "false";
            var defaultValue = settingElement.XPathSelectElement("defaultValue")?.Value;

            if(name == null)
            {
                throw new XmlException("Failed to load module setting name.");
            }
            if(type == null)
            {
                throw new XmlException("Failed to load module setting type.");
            }
            if(!bool.TryParse(required, out var requiredBool))
            {
                throw new XmlException("Module setting required value is invalid.");
            }
            if(defaultValue == null)
            {
                if(required.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    throw new XmlException("Module setting is not required and thus requires a default value.");
                }
                defaultValue = "";
            }

            return new ModuleSetting(name, description, defaultValue, type, requiredBool);
        }

        private static (string ScriptEngineName, IEnumerable<KeyValuePair<string, string>> EngineArgs) GetModuleScriptEngineInfo(XElement? metadataRoot)
        {
            if (metadataRoot == null)
            {
                throw new XmlException("Failed to find module metadata.");
            }
            var scriptEngineElement = metadataRoot.XPathSelectElement("./scriptEngine");
            if (scriptEngineElement == null)
            {
                throw new XmlException("Failed to find module script engine metadata.");
            }

            var name = scriptEngineElement.XPathSelectElement("./name")?.Value;
            if (name == null)
            {
                throw new XmlException("Failed to find module script engine.");
            }

            var argsElement = scriptEngineElement.XPathSelectElement("args");
            var args = new Dictionary<string, string>();
            if(argsElement != null)
            {
                args = argsElement.Attributes().ToDictionary(a => a.Name.ToString(), a => a.Value);
            }
            return (name, args);
        }

        private static (string Name, string Version, string Description) GetCommonMetadataValues(XElement? metadataRoot)
        {
            if(metadataRoot == null)
            {
                throw new XmlException("Failed to load metadata.");
            }

            var name = metadataRoot.XPathSelectElement("./name")?.Value;
            var version = metadataRoot.XPathSelectElement("./version")?.Value;
            var description = metadataRoot.XPathSelectElement("./description")?.Value;

            if(name == null)
            {
                throw new XmlException("Package metadata must contain a package name.");
            }
            if(version == null)
            {
                version = string.Empty;
            }
            if(description == null)
            {
                description = string.Empty;
            }

            return (name, version, description);
        }
    }
}
