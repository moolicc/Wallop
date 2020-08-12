using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.IO;
using System.Reflection;

namespace Wallop.Presenter.Scripting
{
    public class CsModule : Bridge.Module
    {
        private Func<Controller> getController;

        //TODO: These need to not be Func<>s but a custom delegate type.
        public Func<Controller> GetController
        {
            get => getController;
            set => getController = value;
        }

        public CsModule(Bridge.Manifest manifest)
            : base(manifest)
        {

        }

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

                    options = options.AddReferences(Directory.GetFiles(Manifest.Directory, "*.dll",
                            SearchOption.TopDirectoryOnly))
                            .AddReferences(Assembly.GetExecutingAssembly())
                            .AddImports("Wallop", "Wallop.Scripting", "System", "System.Linq", "System.IO");



                    var script = CSharpScript.Create("", options: options, globalsType: GetType());
                    var state = script.RunAsync(globals: this).Result;

                    state = state.ContinueWithAsync(System.IO.File.ReadAllText(Manifest.Directory + Manifest.SourceFile), options: options).Result;

                    CSharpScript.RunAsync(System.IO.File.ReadAllText(Manifest.SourceFile), globals: this, options: options)
                        .Wait();
                    if (GetController == null)
                    {
                        //TODO: Error
                    }
                }

            }
            catch (Exception e)
            {
                //TODO
            }
        }

        public static Controller CreateController(Bridge.Module module)
        {
            return ((CsModule)module).GetController.Invoke();
        }
    }
}
