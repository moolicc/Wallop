using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WallApp.Scripting
{
    public abstract class Module
    {
        public Version Version { get; private set; }

        public string File { get; private set; }
        public string SourceFile { get; private set; }
        public string ViewSourceFile { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public int MinWidth { get; private set; }
        public int MinHeight { get; private set; }
        public int MaxWidth { get; private set; }
        public int MaxHeight { get; private set; }

        public bool AllowsCustomEffect { get; private set; }


        internal void Init(Version version, string file, string sourceFile, string viewSourceFile, string name, string description, int minWidth, int minHeight, int maxWidth, int maxHeight, bool allowsCustomEffects)
        {
            Version = version;
            File = file;
            ViewSourceFile = viewSourceFile;
            SourceFile = sourceFile;
            Name = name;
            Description = description;

            MinWidth = minWidth;
            MinHeight = minHeight;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;

            AllowsCustomEffect = allowsCustomEffects;

            Initialize();
        }

        public abstract Controller CreateController();
        public abstract object CreateViewModel(LayerSettings settings);

        protected abstract void Initialize();
    }
}
