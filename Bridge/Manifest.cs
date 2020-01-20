using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace WallApp.Bridge
{
    public class Manifest
    {
        public Version Version { get; private set; }

        public string ManifestFile { get; private set; }
        public string SourceFile { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public int MinWidth { get; private set; }
        public int MinHeight { get; private set; }
        public int MaxWidth { get; private set; }
        public int MaxHeight { get; private set; }

        public bool AllowsCustomEffect { get; private set; }

        public string Directory => System.IO.Path.GetDirectoryName(SourceFile);

        private Manifest()
            : this(new Version(0, 0, 0, 0), "manifest", "source", "name", "description", 0, 0, 1, 1, false)
        {

        }

        public Manifest(Version version, string manifestFile, string sourceFile, string name, string description, int minWidth, int minHeight, int maxWidth, int maxHeight, bool allowsCustomEffects)
        {
            Version = version;
            ManifestFile = manifestFile;
            SourceFile = sourceFile;
            Name = name;
            Description = description;
            MinWidth = minWidth;
            MinHeight = minHeight;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            AllowsCustomEffect = allowsCustomEffects;
        }

        public static IEnumerable<Manifest> LoadManifests(string topDirectory)
        {
            //TODO: Error handling
            var directories = System.IO.Directory.GetDirectories(topDirectory);
            foreach (var item in directories)
            {
                foreach (var xmlFile in System.IO.Directory.GetFiles(item, "*.xml"))
                {
                    yield return LoadManifest(xmlFile);
                }
            }
        }

        private static Manifest LoadManifest(string manifestFile)
        {
            var manifest = new Manifest();
            manifest.ManifestFile = manifestFile;

            var doc = XDocument.Load(manifestFile);
            var root = doc.Root;
            foreach (var element in root.Elements())
            {
                if(element.Name == "name")
                {
                    manifest.Name = element.Value;
                }
                if (element.Name == "description")
                {
                    manifest.Description = element.Value;
                }
                if (element.Name == "version")
                {
                    manifest.Version = Version.Parse(element.Value);
                }
                if (element.Name == "source")
                {
                    manifest.SourceFile = element.Value;
                }
                if (element.Name == "minWidth")
                {
                    manifest.MinWidth = int.Parse(element.Value);
                }
                if (element.Name == "minheight")
                {
                    manifest.MinHeight = int.Parse(element.Value);
                }
                if (element.Name == "maxWidth")
                {
                    manifest.MaxWidth = int.Parse(element.Value);
                }
                if (element.Name == "maxheight")
                {
                    manifest.MaxHeight = int.Parse(element.Value);
                }
            }

            return manifest;
        }
    }
}
