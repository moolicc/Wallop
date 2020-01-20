using System;

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
    }
}
