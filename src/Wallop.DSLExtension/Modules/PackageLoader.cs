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
            var fileInfo = new FileInfo(file);
            var baseDir = fileInfo.Directory?.FullName ?? "";
            var packageInfo = LoadPackageInfo(file, root.XPathSelectElement("./metadata"));
            var modules = LoadModules(packageInfo, root.XPathSelectElement("./modules"), baseDir);

            return new Package()
            {
                Info = packageInfo,
                DeclaredModules = modules.ToArray()
            };
        }

        private static PackageInfo LoadPackageInfo(string xmlFile, XElement? rootElement)
        {
            if (rootElement == null)
            {
                throw new XmlException("Failed to find package metadata.");
            }
            var commonData = GetCommonMetadataValues(rootElement);
            return new PackageInfo(xmlFile,
                ApplyVariables(commonData.Name, commonData.Variables, null),
                ApplyVariables(commonData.Version, commonData.Variables, null),
                ApplyVariables(commonData.Description, commonData.Variables, null),
                commonData.Variables);
        }

        private static IEnumerable<Module> LoadModules(PackageInfo packageInfo, XElement? rootElement, string baseDir)
        {
            if (rootElement == null)
            {
                throw new XmlException("Failed to find package module node.");
            }
            foreach (var element in rootElement.XPathSelectElements("./module"))
            {
                yield return LoadModule(packageInfo, element, baseDir);
            }
        }

        private static Module LoadModule(PackageInfo packageInfo, XElement? moduleElement, string baseDir)
        {
            if (moduleElement == null)
            {
                throw new XmlException("Failed to load module from package.");
            }

            var info = LoadModuleInfo(packageInfo, moduleElement.XPathSelectElement("./metadata"), baseDir);
            var settings = LoadModuleSettings(packageInfo, info, moduleElement.XPathSelectElement("./settings"));
            return new Module()
            {
                ModuleInfo = info,
                ModuleSettings = settings.ToArray() // NOTE: This is to allow us to cache the setting type.
            };
        }

        private static ModuleInfo LoadModuleInfo(PackageInfo packageInfo, XElement? metadataElement, string baseDir)
        {
            if(metadataElement == null)
            {
                throw new XmlException("Failed to load module metadata.");
            }
            var engineInfo = GetModuleScriptEngineInfo(metadataElement);
            var commonMetadata = GetCommonMetadataValues(metadataElement);
            var moduleType = metadataElement?.XPathSelectElement("./type")?.Value;
            var moduleSourceFile = metadataElement?.XPathSelectElement("./source")?.Value;
            var hostApis = GetModuleHostApis(metadataElement?.XPathSelectElement("./apis"));

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

                moduleSourceFile = Path.Combine(baseDir, moduleSourceFile);
                if (!File.Exists(moduleSourceFile))
                {
                    // TODO: Error.
                    throw new XmlException($"Module source file not found. File: {moduleSourceFile}");
                }
            }

            return new ModuleInfo(packageInfo.ManifestPath,
                baseDir,
                ApplyVariables(moduleSourceFile, packageInfo.PackageVariables, commonMetadata.Variables),
                ApplyVariables(commonMetadata.Name, packageInfo.PackageVariables, commonMetadata.Variables),
                ApplyVariables(commonMetadata.Version, packageInfo.PackageVariables, commonMetadata.Variables),
                ApplyVariables(commonMetadata.Description, packageInfo.PackageVariables, commonMetadata.Variables),
                ApplyVariables(engineInfo.ScriptEngineName, packageInfo.PackageVariables, commonMetadata.Variables),
                engineInfo.EngineArgs.Select(kvp
                    => new KeyValuePair<string, string>(ApplyVariables(kvp.Key, packageInfo.PackageVariables, commonMetadata.Variables),
                    ApplyVariables(kvp.Value, packageInfo.PackageVariables, commonMetadata.Variables))),
                typeEnumValue,
                hostApis.Select(api => ApplyVariables(api, packageInfo.PackageVariables, commonMetadata.Variables)),
                commonMetadata.Variables);
        }

        private static IEnumerable<ModuleSetting> LoadModuleSettings(PackageInfo packageInfo, ModuleInfo moduleInfo, XElement? settingRoot)
        {
            if (settingRoot == null)
            {
                yield break;
            }

            foreach (var settingElement in settingRoot.XPathSelectElements("./setting"))
            {
                var setting = LoadModuleSetting(packageInfo, moduleInfo, settingElement);
                if(setting != null)
                {
                    yield return setting;
                }
            }
        }

        private static ModuleSetting? LoadModuleSetting(PackageInfo packageInfo, ModuleInfo moduleInfo, XElement? settingElement)
        {
            if (settingElement == null)
            {
                return null;
            }

            var name = settingElement.XPathSelectElement("name")?.Value;
            var description = settingElement.XPathSelectElement("description")?.Value ?? "";
            var typeElement = settingElement.XPathSelectElement("type");
            var required = settingElement.XPathSelectElement("required")?.Value ?? "false";
            var tracked = settingElement.XPathSelectElement("tracked")?.Value ?? "false";
            var defaultValue = settingElement.XPathSelectElement("defaultValue")?.Value;
            var bindingElements = settingElement.XPathSelectElements("binding");
            IEnumerable<ModuleSettingBinding> bindings = Array.Empty<ModuleSettingBinding>();

            if(name == null)
            {
                throw new XmlException("Failed to load module setting name.");
            }
            if(typeElement == null)
            {
                throw new XmlException("Failed to load module setting type.");
            }
            if(!bool.TryParse(required, out var requiredBool))
            {
                throw new XmlException("Module setting required value is invalid.");
            }
            if (!bool.TryParse(tracked, out var trackedBool))
            {
                throw new XmlException("Module setting tracked value is invalid.");
            }
            if (defaultValue == null)
            {
                if(required.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    throw new XmlException("Module setting is not required and thus requires a default value.");
                }
            }
            else
            {
                defaultValue = ApplyVariables(defaultValue, packageInfo, moduleInfo);
            }
            if(bindingElements != null)
            {
                bindings = LoadBindings(packageInfo, moduleInfo, bindingElements);
            }

            var type = ApplyVariables(typeElement.Value, packageInfo, moduleInfo);
            var typeArgs = typeElement.Attributes().Select(a
                => new KeyValuePair<string, string>(
                    ApplyVariables(a.Name.ToString(), packageInfo, moduleInfo),
                    ApplyVariables(a.Value, packageInfo, moduleInfo)));

            return new ModuleSetting(ApplyVariables(name, packageInfo, moduleInfo),
                ApplyVariables(description, packageInfo, moduleInfo),
                defaultValue,
                type,
                requiredBool,
                trackedBool,
                bindings,
                typeArgs);
        }

        private static IEnumerable<ModuleSettingBinding> LoadBindings(PackageInfo packageInfo, ModuleInfo moduleInfo, IEnumerable<XElement> bindingElements)
        {
            const string TYPE_PROPERTY_DELIMITER = ":";

            foreach (var element in bindingElements)
            {
                if(!element.Value.Contains(TYPE_PROPERTY_DELIMITER))
                {
                    throw new XmlException("Module setting binding is not in a proper format.");
                }
                var delimiterIndex = element.Value.IndexOf(TYPE_PROPERTY_DELIMITER);
                var type = element.Value.Substring(0, delimiterIndex);
                var property = element.Value.Substring(delimiterIndex + 1);
                yield return new ModuleSettingBinding(
                    ApplyVariables(type, packageInfo, moduleInfo),
                    ApplyVariables(property, packageInfo, moduleInfo));
            }
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

        private static IEnumerable<string> GetModuleHostApis(XElement? apiRoot)
        {
            if(apiRoot == null)
            {
                return Array.Empty<string>();
            }

            var subElements = apiRoot.XPathSelectElements("./api");
            return subElements.Select(e => e.Value);
        }

        private static (string Name, string Version, string Description, IEnumerable<KeyValuePair<string, string>> Variables) GetCommonMetadataValues(XElement? metadataRoot)
        {
            if(metadataRoot == null)
            {
                throw new XmlException("Failed to load metadata.");
            }

            var name = metadataRoot.XPathSelectElement("./name")?.Value;
            var version = metadataRoot.XPathSelectElement("./version")?.Value;
            var description = metadataRoot.XPathSelectElement("./description")?.Value;
            var variables = GetVariableDeclarations(metadataRoot);

            if (name == null)
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

            return (name, version, description, variables);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetVariableDeclarations(XElement metadataRoot)
        {
            var variablesElement = metadataRoot.XPathSelectElement("./variables");

            if(variablesElement == null)
            {
                return Array.Empty<KeyValuePair<string, string>>();
            }
            var ele = variablesElement.Elements();
            return ele.Select(e => GetVariableDeclaration(e));
        }

        private static KeyValuePair<string, string> GetVariableDeclaration(XElement? variableDeclElement)
        {
            if(variableDeclElement == null)
            {
                throw new XmlException("Failed to load package variable declaration.");
            }

            return new KeyValuePair<string, string>(variableDeclElement.Name.ToString(), variableDeclElement.Value);
        }


        private static string ApplyVariables(string input, PackageInfo packageInfo, ModuleInfo? moduleInfo)
            => ApplyVariables(input, packageInfo.PackageVariables, moduleInfo?.Variables);

        private static string ApplyVariables(string input, IEnumerable<KeyValuePair<string, string>> packageVariables, IEnumerable<KeyValuePair<string, string>>? moduleVariables)
        {
            if(moduleVariables != null)
            {
                foreach (var item in moduleVariables)
                {
                    input = input.Replace($"${item.Key}", item.Value);
                }
            }

            foreach (var item in packageVariables)
            {
                input = input.Replace($"${item.Key}", item.Value);
            }

            return input;
        }
    }
}
