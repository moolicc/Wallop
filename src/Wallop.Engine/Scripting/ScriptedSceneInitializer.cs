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
using Wallop.Engine.Types.Plugins.EndPoints;

namespace Wallop.Engine.Scripting
{
    internal class ScriptedSceneInitializer
    {
        public ScriptHostFunctions ScriptHostFunctions { get; set; }
        public Scene Scene { get; private set; }
        public PluginContext PluginContext { get; private set; }
        public IEnumerable<IScriptEngineProvider> ScriptEngineProviders { get; private set; }
        public Dictionary<string, Type> BindableComponentTypes { get; private set; }
        public TaskHandlerProvider TaskProvider { get; private set; }

        public ScriptedSceneInitializer(ScriptHostFunctions hostFunctions, Scene scene, PluginContext pluginContext, TaskHandlerProvider taskProvider, IEnumerable<IScriptEngineProvider> scriptEngineProviders, IEnumerable<KeyValuePair<string, Type>> bindableComponentTypes)
        {
            ScriptHostFunctions = hostFunctions;
            Scene = scene;
            PluginContext = pluginContext;
            TaskProvider = taskProvider;
            ScriptEngineProviders = scriptEngineProviders;
            BindableComponentTypes = new Dictionary<string, Type>(bindableComponentTypes);
        }

        public void InitializeDirectorScripts()
        {
            EngineLog.For<ScriptedSceneInitializer>().Info("Initializing director scripts...");
            foreach (var director in Scene.Directors)
            {
                if(director is ScriptedDirector scriptedDirector)
                {
                    EngineLog.For<ScriptedSceneInitializer>().Debug("Running director initialization for {director}...", director.Name);
                    InitializeElement(scriptedDirector, (ctx) => BuildDirectorContext(ctx, scriptedDirector));
                }
            }
            EngineLog.For<ScriptedSceneInitializer>().Debug("Waiting for director script initialization to complete...");
            foreach (var director in Scene.Directors)
            {
                if (director is ScriptedDirector scriptedDirector)
                {
                    scriptedDirector.WaitForExecuteAsync().WaitAndCall(scriptedDirector, (e, d)
                        => EngineLog.For<ScriptedSceneInitializer>().Error(e, "Failed to initialize director script! Director: {director}, Message: {message}, Inner message: {innermessage}, Script: {script}.", d.Name, e.Message, e.InnerException?.Message, d.ModuleDeclaration.ModuleInfo.SourcePath));
                }
            }
            EngineLog.For<ScriptedSceneInitializer>().Info("***** Director scripts initialization complete! *****");
        }

        public void InitializeActorScripts()
        {
            foreach (var layout in Scene.Layouts)
            {
                var actors = layout.EcsRoot.GetActors<ScriptedActor>();
                EngineLog.For<ScriptedSceneInitializer>().Info("Initializing actor scripts for layout {layout}...", layout.Name);
                InitializeActors(layout, actors);
            }
            EngineLog.For<ScriptedSceneInitializer>().Info("***** Actor scripts initialization complete! *****");
        }

        private void InitializeActors(Layout rootLayout, IEnumerable<ScriptedActor> actors)
        {
            foreach (var actor in actors)
            {
                EngineLog.For<ScriptedSceneInitializer>().Debug("Running actor initialization for {actor}...", actor.Id);
                InitializeElement(actor, (ctx) => BuildActorContext(ctx, rootLayout, actor));
                EngineLog.For<ScriptedSceneInitializer>().Debug("Creating bindings for {actor}...", actor.Id);
                InitializeSettingBindings(actor, actor.GetAttachedScriptContext());
            }
            EngineLog.For<ScriptedSceneInitializer>().Debug("Waiting for actor script initialization to complete...");
            foreach (var actor in actors)
            {
                actor.WaitForExecuteAsync().WaitAndCall(actor, (e, a)
                    => EngineLog.For<ScriptedSceneInitializer>().Error(e, "Failed to initialize actor script! Actor: {actor}, Message: {message}, Inner message: {innermessage}, Script: {script}.", a.Id, e.Message, e.InnerException?.Message, a.ModuleDeclaration.ModuleInfo.SourcePath));
            }
        }

        private void InitializeSettingBindings(ScriptedActor actor, IScriptContext context)
        {
            Dictionary<Type, BindableType> bindingInstances = new Dictionary<Type, BindableType>();

            // Load bindings defined in the module.
            foreach (var setting in actor.ModuleDeclaration.ModuleSettings)
            {
                foreach (var binding in setting.Bindings)
                {
                    EngineLog.For<ScriptedSceneInitializer>().Debug("Setting up binding {binding} on setting {setting}.", binding, setting);

                    if(!BindableComponentTypes.TryGetValue(binding.TypeName, out var bindingType))
                    {
                        EngineLog.For<ScriptedSceneInitializer>().Warn("Binding not found on actor {actor}! Binding: {binding}.", actor.Id, binding.TypeName);
                        continue;
                    }


                    if (bindingInstances.TryGetValue(bindingType, out var instance))
                    {
                        EngineLog.For<ScriptedSceneInitializer>().Debug("Binding added onto existing bindable for actor {actor} from {setting} to concrete member {property} via {binding}.", actor.Id, setting.SettingName, binding.PropertyName, binding.TypeName);
                        instance.BindProperty(binding.PropertyName, setting.SettingName);
                    }
                    else
                    {
                        var newBindable = (BindableType?)Activator.CreateInstance(bindingType);
                        if (newBindable == null)
                        {
                            EngineLog.For<ScriptedSceneInitializer>().Warn("Binding failed instantiation for actor {actor}! Binding: {binding}.", actor.Id, binding.TypeName);
                            continue;
                        }

                        newBindable.Bind(context);
                        newBindable.BindProperty(binding.PropertyName, setting.SettingName);

                        bindingInstances.Add(bindingType, newBindable);
                        actor.Components.Add(newBindable);


                        EngineLog.For<ScriptedSceneInitializer>().Debug("Binding created on actor {actor} from {setting} to concrete member {property} via {binding}.", actor.Id, setting.SettingName, binding.PropertyName, binding.TypeName);
                    }
                }
            }

            // Load bindings defined in the loaded layoutdefinition/actormodules settings.
            foreach (var binding in actor.StoredDefinition.StoredBindings)
            {
                if (!BindableComponentTypes.TryGetValue(binding.TypeName, out var bindingType))
                {
                    // TODO: Error / Warning
                    continue;
                }

                if (bindingInstances.TryGetValue(bindingType, out var instance))
                {
                    instance.BindProperty(binding.PropertyName, binding.SettingName);
                }
                else
                {
                    var newBindable = (BindableType?)Activator.CreateInstance(bindingType);
                    if (newBindable == null)
                    {
                        // TODO: Error / Warning
                        continue;
                    }

                    newBindable.Bind(context);
                    newBindable.BindProperty(binding.PropertyName, binding.SettingName);

                    bindingInstances.Add(bindingType, newBindable);
                    actor.Components.Add(newBindable);
                }
            }
        }

        private void InitializeElement(ScriptedElement component, Action<IScriptContext> mutateScriptContext)
        {
            var engineProvider = ScriptEngineProviders.FirstOrDefault(ep => ep.Name == component.ModuleDeclaration.ModuleInfo.ScriptEngineId);
            if (engineProvider == null)
            {
                EngineLog.For<ScriptedSceneInitializer>().Error("Failed to find ScriptEngineProvider {engine} from module {module} for ECS element {element}.", component.ModuleDeclaration.ModuleInfo.ScriptEngineId, component.ModuleDeclaration.ModuleInfo.Id, component.Name);
                return;
            }

            string sourceFullpath = component.ModuleDeclaration.ModuleInfo.SourcePath;

            EngineLog.For<ScriptedSceneInitializer>().Info("Creating module script resources for {element} from source {filepath}...", component.Name, sourceFullpath);
            var source = File.ReadAllText(sourceFullpath);

            EngineLog.For<ScriptedSceneInitializer>().Debug("Creating script components...");
            var engine = engineProvider.CreateScriptEngine(component.ModuleDeclaration.ModuleInfo.ScriptEngineArgs);
            var context = engineProvider.CreateContext();

            EngineLog.For<ScriptedSceneInitializer>().Debug("Attaching script components...");
            engine.AttachContext(context);

            EngineLog.For<ScriptedSceneInitializer>().Debug("Injecting system host API...");
            ScriptHostFunctions.Inject(context, component.ModuleDeclaration, component);

            EngineLog.For<ScriptedSceneInitializer>().Debug("Caller mutating script context...");
            mutateScriptContext(context);

            EngineLog.For<ScriptedSceneInitializer>().Debug("Injecting script settings into context...");
            AddSettingsToContext(context, component);

            EngineLog.For<ScriptedSceneInitializer>().Debug("Plugins mutating script context...");
            AddInjectionsToContext(context, component);

            EngineLog.For<ScriptedSceneInitializer>().Debug("Plugins injecting apis into script context...");
            SetupHostApisForContext(context, component);

            EngineLog.For<ScriptedSceneInitializer>().Debug("Performing validation over script context...");
            ValidateContext(context, component);


            EngineLog.For<ScriptedSceneInitializer>().Debug("Executing script initialization...");
            component.InitializeScript(TaskProvider, engine, source);

            EngineLog.For<ScriptedSceneInitializer>().Debug("Setting up ECS element callback scene triggers...");
            component.BeforeUpdateCallback = Scene.OnBeforeScriptedElementUpdate;
            component.AfterUpdateCallback = Scene.OnAfterScriptedElementUpdate;
            component.BeforeDrawCallback = Scene.OnBeforeScriptedElementDraw;
            component.AfterDrawCallback = Scene.OnAfterScriptedElementDraw;
            component.PanicCallback = Scene.OnScriptedElementPanic;


            EngineLog.For<ScriptedSceneInitializer>().Info("Script initialized!");
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

        private void AddSettingsToContext(IScriptContext context, ScriptedElement component)
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
                context.SetValue(setting.Key, value);
            }

        }
        
        private void AddInjectionsToContext(IScriptContext context, ScriptedElement component)
        {
            // Execute the injection endpoint.
            EngineLog.For<ScriptedSceneInitializer>().Info("Calling " + nameof(ScriptInjectEndPoint) + " plugin end point...");
            var endPointContext = new ScriptInjectEndPoint(component.ModuleDeclaration, context);
            PluginContext.ExecuteEndPoint<IInjectScriptContextEndPoint>(endPointContext);

            PluginContext.WaitForEndPointExecutionAsync<IInjectScriptContextEndPoint>().WaitAndCall(component, (e, c)
                => EngineLog.For<ScriptedSceneInitializer>().Error(e, "Plugin execution error on ECS element {element}!", c.Name));
        }

        private void SetupHostApisForContext(IScriptContext context, ScriptedElement component)
        {
            // Enumerate all HostAPI plugins and for each that this component uses, run the entry point.
            var hostApis = PluginContext.GetImplementations<IHostApi>();
            foreach (var targetApi in component.ModuleDeclaration.ModuleInfo.HostApis)
            {
                EngineLog.For<ScriptedSceneInitializer>().Debug("Using HostAPI {api} on ECS element {element}...", targetApi, component.Name);
                var api = hostApis.FirstOrDefault(h => h.Name == targetApi);

                if (api == null)
                {
                    EngineLog.For<ScriptedSceneInitializer>().Warn("HostAPI {api} not found!", targetApi);
                    continue;
                }

                api.Use(context);
            }
        }

        private void ValidateContext(IScriptContext context, ScriptedElement component)
        {
            foreach (var declaredSetting in component.ModuleDeclaration.ModuleSettings)
            {
                if(declaredSetting.Required && !context.ContainsValue(declaredSetting.SettingName))
                {
                    EngineLog.For<ScriptedSceneInitializer>().Warn("ECS element {element} is missing required module setting {setting}!", component.Name, declaredSetting.SettingName);
                    continue;
                }

                if(!declaredSetting.Required && !context.ContainsValue(declaredSetting.SettingName))
                {
                    object? value = declaredSetting.DefaultValue;

                    // Deserialize the value as appropriate for the type of setting.
                    if (declaredSetting.CachedType != null)
                    {
                        if (!declaredSetting.CachedType.TryDeserialize(declaredSetting.DefaultValue, out value, declaredSetting.SettingTypeArgs))
                        {
                            EngineLog.For<ScriptedSceneInitializer>().Warn("ECS element {element} attempted to use default setting value {value} for setting {setting}, but deserialization failed! ", component.Name, value, declaredSetting.SettingName);
                            break;
                        }
                    }

                    EngineLog.For<ScriptedSceneInitializer>().Debug("Added setting value ({setting} = {value}) to script context...", declaredSetting.SettingName, value);
                    context.SetValue(declaredSetting.SettingName, value);
                }
            }
        }
    }
}
