using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace WallApp.Scripting.Cs
{
    public class CsModule : Module
    {
        public Func<LayerSettings, Panel> GetOptionsControl { get; set; }
        public Func<Controller> GetController { get; set; }
        
        
        internal override void Init(string file, string sourceFile, string name, string description)
        {
            base.Init(file, sourceFile, name, description);
            try
            {
                var options = ScriptOptions.Default
                    .AddReferences(AppDomain.CurrentDomain.GetAssemblies()
                        .Select(a => a.ManifestModule.FullyQualifiedName))
                    .AddImports("WallApp", "WallApp.Scripting", "System", "System.Linq", "System.IO")
                    .AddImports("Microsoft.Xna.Framework", "Microsoft.Xna.Framework.Graphics")
                    .AddImports("System.Windows.Forms")
                    .AddReferences(Directory.GetFiles(Path.GetDirectoryName(file), "*.dll", SearchOption.TopDirectoryOnly));
                

                CSharpScript.RunAsync(System.IO.File.ReadAllText(sourceFile), globals: this, options: options).Wait();
                if (GetController == null)
                {
                    //TODO: Error
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override string GetName()
        {
            return Name;
        }

        public override string GetDescription()
        {
            return Description;
        }

        public override Panel GetOptionsPanel(LayerSettings layerSettings)
        {
            return GetOptionsControl?.Invoke(layerSettings);
        }

        public override Controller CreateController()
        {
            return GetController?.Invoke();
        }
    }
}
