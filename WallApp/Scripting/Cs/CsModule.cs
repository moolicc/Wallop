using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace WallApp.Scripting.Cs
{
    public class CsModule : Module
    {
        //TODO: These need to not be Func<>s but a custom delegate type.
        public Func<Controller> GetController { get; set; }
        public Func<LayerSettings, object> GetViewModel { get; set; }

        protected override void Initialize()
        {
            
            try
            {
                var options = ScriptOptions.Default;
                
                using (var interactiveLoader = new InteractiveAssemblyLoader())
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        interactiveLoader.RegisterDependency(assembly);
                    }

                    options = options.AddReferences(Directory.GetFiles(Path.GetDirectoryName(File), "*.dll",
                            SearchOption.TopDirectoryOnly))
                            .AddReferences(Assembly.GetExecutingAssembly())
                            .AddImports("WallApp", "WallApp.Scripting", "System", "System.Linq", "System.IO")
                            .AddImports("Microsoft.Xna.Framework", "Microsoft.Xna.Framework.Graphics",
                            "Microsoft.Xna.Framework.Input")
                            .AddImports("System.Windows", "System.ComponentModel");
                    


                    var script = CSharpScript.Create("", options: options, globalsType: GetType());
                    var state = script.RunAsync(globals: this).Result;

                    state = state.ContinueWithAsync(System.IO.File.ReadAllText(SourceFile), options: options).Result;

                    CSharpScript.RunAsync(System.IO.File.ReadAllText(SourceFile), globals: this, options: options)
                        .Wait();
                    if (GetController == null)
                    {
                        //TODO: Error
                    }
                }

            }
            catch (Exception e)
            {
                //throw e;
            }
        }

        public override Controller CreateController()
        {
            return GetController?.Invoke();
        }

        public override object CreateViewModel(LayerSettings settings)
        {
            if(GetViewModel == null)
            {
                return null;
            }
            return GetViewModel(settings);
        }
    }
}
