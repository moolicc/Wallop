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
        public Func<SettingsController> GetSettingsController { get; set; }
        public Func<Controller> GetController { get; set; }
        
        protected override void Initialize()
        {
            try
            {
                var options = ScriptOptions.Default
                    .AddReferences(AppDomain.CurrentDomain.GetAssemblies()
                        .Select(a => a.ManifestModule.FullyQualifiedName))
                    .AddImports("WallApp", "WallApp.Scripting", "System", "System.Linq", "System.IO")
                    .AddImports("Microsoft.Xna.Framework", "Microsoft.Xna.Framework.Graphics")
                    .AddImports("System.Windows.Forms")
                    .AddReferences(Directory.GetFiles(Path.GetDirectoryName(File), "*.dll", SearchOption.TopDirectoryOnly));


                CSharpScript.RunAsync(System.IO.File.ReadAllText(SourceFile), globals: this, options: options).Wait();
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

        public override SettingsController CreateSettingsController()
        {
            return GetSettingsController?.Invoke();
        }

        public override Controller CreateController()
        {
            return GetController?.Invoke();
        }
    }
}
