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
        ScriptHostFunctions ScriptHostFunctions { get; set; }
        public Scene Scene { get; private set; }
        public PluginContext PluginContext { get; private set; }
        public IEnumerable<IScriptEngineProvider> ScriptEngineProviders { get; private set; }

        public ScriptedSceneInitializer(ScriptHostFunctions hostFunctions, Scene scene, PluginContext pluginContext, IEnumerable<IScriptEngineProvider> scriptEngineProviders)
        {
            ScriptHostFunctions = hostFunctions;
            Scene = scene;
            PluginContext = pluginContext;
            ScriptEngineProviders = scriptEngineProviders;
        }

        public void InitializeDirectorScripts()
        {
            foreach (var director in Scene.Directors)
            {
                if(director is ScriptedDirector scriptedDirector)
                {
                    InitializeComponent(scriptedDirector, (ctx) => BuildDirectorContext(ctx, scriptedDirector));
                }
            }
            foreach (var director in Scene.Directors)
            {
                if (director is ScriptedDirector scriptedDirector)
                {
                    scriptedDirector.WaitForExecuteAsync().WaitAndThrow();
                }
            }
        }

        public void InitializeActorScripts()
        {
            foreach (var layout in Scene.Layouts)
            {
                var actors = layout.EcsRoot.GetActors<ScriptedActor>();
                InitializeActors(layout, actors);
            }
        }

        private void InitializeActors(Layout rootLayout, IEnumerable<ScriptedActor> actors)
        {
            foreach (var actor in actors)
            {
                InitializeComponent(actor, (ctx) => BuildActorContext(ctx, rootLayout, actor));
            }
            foreach (var actor in actors)
            {
                actor.WaitForExecuteAsync().WaitAndThrow();
            }
        }

        private void InitializeComponent(ScriptedEcsComponent component, Action<IScriptContext> mutateScriptContext)
        {
            var engineProvider = ScriptEngineProviders.FirstOrDefault(ep => ep.Name == component.ModuleDeclaration.ModuleInfo.ScriptEngineId);
            if (engineProvider == null)
            {
                // TODO:
                return;
            }

            string sourceFullpath = component.ModuleDeclaration.ModuleInfo.SourcePath;

            var source = File.ReadAllText(sourceFullpath);

            var engine = engineProvider.CreateScriptEngine(component.ModuleDeclaration.ModuleInfo.ScriptEngineArgs);
            var context = engineProvider.CreateContext();

            engine.AttachContext(context);
            ScriptHostFunctions.Inject(context, component.ModuleDeclaration, component);
            mutateScriptContext(context);
            AddSettingsToContext(context, component);
            AddInjectionsToContext(context, component);
            SetupHostApisForContext(context, component);
            ValidateContext(context, component);

            component.InitializeScript(engine, source);

            component.BeforeUpdateCallback = Scene.OnBeforeScriptedComponentUpdate;
            component.AfterUpdateCallback = Scene.OnAfterScriptedComponentUpdate;
            component.BeforeDrawCallback = Scene.OnBeforeScriptedComponentDraw;
            component.AfterDrawCallback = Scene.OnAfterScriptedComponentDraw;


        }

        private void BuildDirectorContext(IScriptContext context, ScriptedDirector director)
        {
            //context.AddReadonlyProperty<ScriptContextExtensions.ExplicitGetters.GetGlInstance>(nameof(GLInstance), this);
            //context.AddDelegate(ScriptContextExtensions.MemberNames.GET_BASE_DIRECTORY, new ScriptContextExtensions.ExplicitGetters.GetBaseDirectory(() =>
            //{
            //    var dir = new FileInfo(director.ModuleDeclaration.ModuleInfo.SourcePath);
            //    return dir.Directory.OrThrow().FullName;
            //}));


        }

        private void BuildActorContext(IScriptContext context, Layout owningLayout, ScriptedActor actor)
        {
            // Add variables and functions.
            //context.AddReadonlyProperty<ScriptContextExtensions.ExplicitGetters.GetGlInstance>(nameof(GLInstance), this, ScriptContextExtensions.MemberNames.GET_GL_INSTANCE);
            //context.AddReadonlyProperty<ScriptContextExtensions.ExplicitGetters.GetRenderSize>(nameof(Layout.RenderSize), owningLayout, ScriptContextExtensions.MemberNames.GET_RENDER_SIZE);
            //context.AddReadonlyProperty<ScriptContextExtensions.ExplicitGetters.GetActualSize>(nameof(Layout.ActualSize), owningLayout, ScriptContextExtensions.MemberNames.GET_ACTUAL_SIZE);
            //context.AddDelegate(ScriptContextExtensions.MemberNames.GET_BASE_DIRECTORY, new ScriptContextExtensions.ExplicitGetters.GetBaseDirectory(() =>
            //{
            //    var dir = new FileInfo(actor.ModuleDeclaration.ModuleInfo.SourcePath);
            //    return dir.Directory.OrThrow().FullName;
            //}));


        }

        private void AddSettingsToContext(IScriptContext context, ScriptedEcsComponent component)
        {
            // Add defined setting values.
            foreach (var setting in component.StoredDefinition.Settings)
            {
                object? value = setting.Value;

                // Deserialize the value as appropriate for the type of setting.
                foreach (var declaredSetting in component.ModuleDeclaration.ModuleSettings)
                {
                    if (declaredSetting.SettingName == setting.Key)
                    {
                        if (declaredSetting.CachedType == null)
                        {
                            break;
                        }
                        if (!declaredSetting.CachedType.TryDeserialize(setting.Value, out value, declaredSetting.SettingTypeArgs))
                        {
                            // TODO: Message
                            break;
                        }
                    }
                }
                context.AddValue(setting.Key, value);
            }

        }
        
        private void AddInjectionsToContext(IScriptContext context, ScriptedEcsComponent component)
        {
            // Execute the injection endpoint.
            var endPointContext = new Types.Plugins.EndPoints.ScriptInjectEndPoint(component.ModuleDeclaration, context);
            PluginContext.ExecuteEndPoint<IInjectScriptContextEndPoint>(endPointContext);
            PluginContext.WaitForEndPointExecutionAsync<IInjectScriptContextEndPoint>().WaitAndThrow();
        }

        private void SetupHostApisForContext(IScriptContext context, ScriptedEcsComponent component)
        {
            // Enumerate all HostAPI plugins and for each that this component uses, run the entry point.
            var hostApis = PluginContext.GetImplementations<IHostApi>();
            foreach (var targetApi in component.ModuleDeclaration.ModuleInfo.HostApis)
            {
                var api = hostApis.First(h => h.Name == targetApi);
                api.Use(context);
            }
        }

        private void ValidateContext(IScriptContext context, ScriptedEcsComponent component)
        {
            foreach (var declaredSetting in component.ModuleDeclaration.ModuleSettings)
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
