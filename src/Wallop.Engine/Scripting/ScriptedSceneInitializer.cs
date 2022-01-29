using PluginPantry;
using Silk.NET.OpenGL;
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
using Wallop.Engine.Types.Plugins;

namespace Wallop.Engine.Scripting
{
    internal class ScriptedSceneInitializer
    {
        public GL GLInstance { get; set; }
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
                InitializeActors(layout, actors, actorRunner);
            }
        }

        private void InitializeActors(Layout rootLayout, IEnumerable<ScriptedActor> actors, ScriptedActorRunner actorRunner)
        {
            foreach (var actor in actors)
            {
                var source = string.Empty;
                // TODO: Error handle this.
                source = File.ReadAllText(actor.ControllingModule.ModuleInfo.SourcePath);


                actorRunner.AddActor(actor, actor, (ctx) => BuildActorContext(ctx, rootLayout, actor));

                actorRunner.InvokeExecute(actor.Name, source, false);
            }
            actorRunner.WaitAsync().WaitAndThrow();
        }

        private void BuildActorContext(IScriptContext context, Layout owningLayout, ScriptedActor actor)
        {
            // Add variables and functions.
            context.AddReadonlyProperty<ScriptContextExtensions.Getters.GetGlInstance>(nameof(GLInstance), this);
            context.AddReadonlyProperty<ScriptContextExtensions.Getters.GetRenderSize>(nameof(Layout.RenderSize), owningLayout);
            context.AddReadonlyProperty<ScriptContextExtensions.Getters.GetActualSize>(nameof(Layout.ActualSize), owningLayout);
            context.AddDelegate(ScriptContextExtensions.VariableNames.GET_BASE_DIRECTORY, new ScriptContextExtensions.Getters.GetBaseDirectory(() =>
            {
                var dir = new FileInfo(actor.ControllingModule.ModuleInfo.SourcePath);
                return dir.Directory.OrThrow().FullName;
            }));

            // Add defined setting values.
            foreach (var setting in actor.StoredDefinition.Settings)
            {
                object? value = setting.Value;

                // Deserialize the value as appropriate for the type of setting.
                foreach (var declaredSetting in actor.ControllingModule.ModuleSettings)
                {
                    if(declaredSetting.SettingName == setting.Key)
                    {
                        if(declaredSetting.CachedType == null)
                        {
                            break;
                        }
                        if(!declaredSetting.CachedType.TryDeserialize(setting.Value, out value, declaredSetting.SettingTypeArgs))
                        {
                            // TODO: Message
                            break;
                        }
                    }
                }
                context.AddValue(setting.Key, value);
            }

            // Execute the injection endpoint.
            var endPointContext = new Types.Plugins.EndPoints.ScriptInjectEndPoint(actor.ControllingModule, context);
            PluginContext.ExecuteEndPoint<IInjectScriptContextEndPoint>(endPointContext);
            PluginContext.WaitForEndPointExecutionAsync<IInjectScriptContextEndPoint>().WaitAndThrow();

            // Enumerate all HostAPI plugins and for each that this actor uses, run the entry point.
            var hostApis = PluginContext.GetImplementations<IHostApi>();
            foreach (var targetApi in actor.ControllingModule.ModuleInfo.HostApis)
            {
                var api = hostApis.First(h => h.Name == targetApi);
                api.Use(context);
            }

            // Validate the context.
            ValidateContext(context, actor);
        }

        private void ValidateContext(IScriptContext context, ScriptedActor actor)
        {
            foreach (var declaredSetting in actor.ControllingModule.ModuleSettings)
            {
                if(declaredSetting.Required && !context.ContainsValue(declaredSetting.SettingName))
                {
                    throw new InvalidOperationException("Actor is missing required module setting.");
                }

                if(!declaredSetting.Required && !context.ContainsValue(declaredSetting.SettingName))
                {
                    object? value = declaredSetting.DefaultValue;

                    // Deserialize the value as appropriate for the type of setting.
                    if (declaredSetting.CachedType != null)
                    {
                        if (!declaredSetting.CachedType.TryDeserialize(declaredSetting.DefaultValue, out value, declaredSetting.SettingTypeArgs))
                        {
                            // TODO: Message
                            break;
                        }
                    }

                    context.AddValue(declaredSetting.SettingName, value);
                }
            }
        }
    }
}
