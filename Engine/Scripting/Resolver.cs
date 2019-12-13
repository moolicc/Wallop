using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Linq;
using WallApp.Engine.Scripting.Cs;

namespace WallApp.Engine.Scripting
{
    class Resolver
    {
        public static readonly Version ManifestVersion = new Version(1, 0);

        public static Dictionary<string, Module> Cache { get; private set; }

        private const string XML_ROOT_NAME = "module";

        static Resolver()
        {
            Cache = new Dictionary<string, Module>();
        }

        public static Module GetCachedModuleFromName(string name)
        {
            foreach (var item in Cache)
            {
                if (item.Value.Name == name)
                {
                    return item.Value;
                }
            }
            throw new KeyNotFoundException();
        }

        public static void LoadModules(string rootDirectory)
        {
            Cache.Clear();
            List<Module> modules = new List<Module>();

            string[] directories = Directory.GetDirectories(rootDirectory);
            foreach (var directory in directories)
            {
                var dir = directory.TrimEnd('\\') + "\\";
                var module = ScanDirectory(dir);
                if (module != null)
                {
                    modules.Add(module);
                    Cache.Add(dir + "manifest.xml", module);
                }
            }
        }

        private static Module ScanDirectory(string directory)
        {
            string manifestPath = directory + "manifest.xml";
            if (!File.Exists(manifestPath))
            {
                //TODO
            }
            return ScanManifest(manifestPath);
        }

        public static Module ScanManifest(string manifestFile, string manifestSource = "")
        {
            XDocument doc;
            if (string.IsNullOrWhiteSpace(manifestSource))
            {
                doc = XDocument.Load(manifestFile);
            }
            else
            {
                doc = XDocument.Parse(manifestSource);
            }
            var root = doc.Root;

            if (root.Name != XML_ROOT_NAME)
            {
                throw new InvalidOperationException("The module's manifest is invalid.");
            }

            var versionAttribute = root.Attribute("version");
            if (versionAttribute == null || !Version.TryParse(versionAttribute.Value, out var v) || v.Major < ManifestVersion.Major)
            {
                throw new InvalidOperationException("The manifest doesn't specify a version or is of an incompatible version.");
            }

            string sourceFile = "";
            string name = "";
            string description = "";
            int minWidth = 0;
            int minHeight = 0;
            int maxWidth = int.MaxValue;
            int maxHeight = int.MaxValue;
            bool allowsCustomEffects = false;
            Version version = new Version(0, 0, 0, 0);

            foreach (var xElement in root.Elements())
            {
                if (xElement.Name == "source")
                {
                    sourceFile = xElement.Value;
                }
                else if (xElement.Name == "name")
                {
                    name = xElement.Value;
                }
                else if (xElement.Name == "description")
                {
                    description = xElement.Value;
                }
                else if (xElement.Name == "minwidth")
                {
                    if (!int.TryParse(xElement.Value, out minWidth))
                    {
                        //TODO: Warning
                    }
                }
                else if (xElement.Name == "minheight")
                {
                    if (!int.TryParse(xElement.Value, out minHeight))
                    {
                        //TODO: Warning
                    }
                }
                else if (xElement.Name == "maxwidth")
                {
                    if (!int.TryParse(xElement.Value, out maxWidth))
                    {
                        //TODO: Warning
                    }
                }
                else if (xElement.Name == "maxheight")
                {
                    if (!int.TryParse(xElement.Value, out maxHeight))
                    {
                        //TODO: Warning
                    }
                }
                else if (xElement.Name == "customeffects")
                {
                    if (!bool.TryParse(xElement.Value, out allowsCustomEffects))
                    {
                        //TODO: Warning
                    }
                }
                else if (xElement.Name == "version")
                {
                    if (!Version.TryParse(xElement.Value, out version))
                    {
                        //TODO: Warning
                    }
                }
            }

            if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(name))
            {
                //TODO: Exception
            }

            if (!File.Exists(sourceFile))
            {
                sourceFile = Path.GetDirectoryName(manifestFile).TrimEnd('\\') + '\\' + sourceFile;
                if (!File.Exists(sourceFile))
                {
                    //TODO
                }
            }

            string kind = Path.GetExtension(sourceFile).TrimStart('.');
            var module = Resolve(kind);
            module.Init(version, manifestFile, sourceFile, name, description, minWidth, minHeight, maxWidth, maxHeight, allowsCustomEffects);
            return module;
        }


        private static Module Resolve(string kind)
        {
            Module module = null;
            if (kind == "csx" || kind == "cs")
            {
                module = new CsModule();
            }
            else
            {
                //TODO: Error.
            }
            return module;
        }
    }
}
