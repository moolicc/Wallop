using PluginPantry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;
using Wallop.DSLExtension.Types.Plugin;
using Wallop.Engine.ECS;
using Wallop.Engine.SceneManagement;
using Wallop.Engine.Scripting.ECS;

namespace Wallop.Engine.Scripting
{
    internal class ScriptedSceneInitializer
    {
        public Scene Scene { get; private set; }
        public PluginContext PluginContext { get; private set; }

        public ScriptedSceneInitializer(Scene scene, PluginContext pluginContext)
        {
            Scene = scene;
            PluginContext = pluginContext;
        }

        public void InitializeScripts(ScriptedActorRunner actorRunner)
        {
            foreach (var layout in Scene.Layouts)
            {
                var actors = layout.EcsRoot.GetActors<ScriptedActor>();
                InitializeActors(actors, actorRunner);
            }
        }

        private void InitializeActors(IEnumerable<ScriptedActor> actors, ScriptedActorRunner actorRunner)
        {
            foreach (var actor in actors)
            {

                var source = string.Empty;
                // TODO: Error handle this.
                source = File.ReadAllText(actor.ControllingModule.ModuleInfo.SourcePath);

                var context = BuildContext(actor);
                ValidateContext(actor, context);

                actorRunner.AddActor(actor, context);
                actorRunner.InvokeExecute(actor.Name, source);
            }
            actorRunner.WaitAsync().WaitAndThrow();
        }

        private ScriptContext BuildContext(ScriptedActor actor)
        {
            var context = new ScriptContext();

            foreach (var item in actor.StoredDefinition.Settings)
            {
                context.AddValue(item.Key, item.Value);
            }

            var endPointContext = new Types.Plugins.EndPoints.ScriptInjectEndPoint(actor.ControllingModule, context);
            PluginContext.ExecuteEndPoint<IInjectScriptContextEndPoint>(endPointContext);
            PluginContext.WaitForEndPointExecutionAsync<IInjectScriptContextEndPoint>().WaitAndThrow();

            ValidateContext(actor, context);
            return context;
        }

        private void ValidateContext(ScriptedActor actor, ScriptContext context)
        {
            foreach (var declaredSetting in actor.ControllingModule.ModuleSettings)
            {
                if(declaredSetting.Required && !context.ExposedVariables.Any(v => v.MemberName == declaredSetting.SettingName))
                {
                    throw new InvalidOperationException("Actor is missing required module setting.");
                }

                if(!declaredSetting.Required && !context.ExposedVariables.Any(v => v.MemberName == declaredSetting.SettingName))
                {
                    context.AddValue(declaredSetting.SettingName, declaredSetting.DefaultValue);
                }
            }
        }
    }
}
