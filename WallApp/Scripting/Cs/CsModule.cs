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
        public Func<SettingsController> GetSettingsController { get; set; }
        public Func<Controller> GetController { get; set; }
        
        protected override void Initialize()
        {
            
            try
            {
                var options = ScriptOptions.Default;

                var mscorlib = typeof(object).GetTypeInfo().Assembly;
                var systemCore = typeof(System.Linq.Enumerable).GetTypeInfo().Assembly;


                //var references = new[] { mscorlib, systemCore };
                //options = options.AddReferences(references);

                using (var interactiveLoader = new InteractiveAssemblyLoader())
                {
                    //foreach (var reference in references)
                    //{
                        //interactiveLoader.RegisterDependency(reference);
                    //}

                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        interactiveLoader.RegisterDependency(assembly);
                    }

                    options.AddReferences(Directory.GetFiles(Path.GetDirectoryName(File), "*.dll",
                            SearchOption.TopDirectoryOnly))
                        .AddImports("WallApp", "WallApp.Scripting", "System", "System.Linq", "System.IO")
                        .AddImports("Microsoft.Xna.Framework", "Microsoft.Xna.Framework.Graphics",
                            "Microsoft.Xna.Framework.Input");


                    var script = CSharpScript.Create("");
                    var state = script.RunAsync(this).Result;

                    //state = state.ContinueWithAsync(System.IO.File.ReadAllText(SourceFile)).Result;

                    //CSharpScript.RunAsync(System.IO.File.ReadAllText(SourceFile), globals: this, options: options)
                        //.Wait();
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
