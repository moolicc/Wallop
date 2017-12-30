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
        public string File { get; private set; }
        public string SourceFile { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        
        internal virtual void Init(string file, string sourceFile, string name, string description)
        {
            File = file;
            SourceFile = sourceFile;
            Name = name;
            Description = description;
        }

        public abstract Panel GetOptionsPanel(LayerSettings layerSettings);
        public abstract string GetName();
        public abstract string GetDescription();
        public abstract Controller CreateController();
    }
}
