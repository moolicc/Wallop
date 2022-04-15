using PluginPantry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;
using Wallop.DSLExtension.Types.Plugin;
using Wallop.Engine.SceneManagement;
using Wallop.Engine.Types.Plugins.EndPoints;

namespace Wallop.Engine.Scripting.ECS.Serialization
{
    public class ElementInitializer
    {
        public static ElementInitializer Instance
        {
            get
            {
                if(_instance == null)
                {
                    throw new InvalidOperationException("Well this is ironic... ElementInitializer not yet initialized.");
                }
                return _instance;
            }
        }

        private static ElementInitializer? _instance;

        private ScriptEngineProviderCache _scriptEngineProviders;
        private TaskHandlerProvider _taskProvider;
        private ScriptHostFunctions _scriptHostFunctions;
        private PluginContext _pluginContext;
        private Dictionary<string, Type> _bindableComponentTypes;

        internal ElementInitializer(ScriptEngineProviderCache scriptEngineProviders, TaskHandlerProvider taskProvider, ScriptHostFunctions scriptHostFunctions, PluginContext pluginContext, IEnumerable<KeyValuePair<string, Type>> bindableComponentTypes)
        {
            _scriptEngineProviders = scriptEngineProviders;
            _taskProvider = taskProvider;
            _scriptHostFunctions = scriptHostFunctions;
            _pluginContext = pluginContext;

            // TODO: Create special "cache" type for this.
            _bindableComponentTypes = new Dictionary<string, Type>(bindableComponentTypes);

            if(_instance != null)
            {
                EngineLog.For<ElementInitializer>().Warn("Double instantiation detected. Reference to previous instance will be forgotten.");
            }
            _instance = this;
        }

        public void InitializeElement(ScriptedElement component, Scene? scene = null)
        {
            var engineProvider = _scriptEngineProviders.Providers.FirstOrDefault(ep => ep.Name == component.ModuleDeclaration.ModuleInfo.ScriptEngineId);
            if (engineProvider == null)
            {
                EngineLog.For<ElementInitializer>().Error("Failed to find ScriptEngineProvider {engine} from module {module} for ECS element {element}.", component.ModuleDeclaration.ModuleInfo.ScriptEngineId, component.ModuleDeclaration.ModuleInfo.Id, component.Name);
                return;
            }

            string sourceFullpath = component.ModuleDeclaration.ModuleInfo.SourcePath;

            EngineLog.For<ElementInitializer>().Info("Creating module script resources for {element} from source {filepath}...", component.Name, sourceFullpath);
            var source = File.ReadAllText(sourceFullpath);

            EngineLog.For<ElementInitializer>().Debug("Creating script components...");
            var engine = engineProvider.CreateScriptEngine(component.ModuleDeclaration.ModuleInfo.ScriptEngineArgs);
            var context = engineProvider.CreateContext();

            EngineLog.For<ElementInitializer>().Debug("Attaching script components...");
            engine.AttachContext(context);

            EngineLog.For<ElementInitializer>().Debug("Injecting system host API...");
            _scriptHostFunctions.Inject(context, component.ModuleDeclaration, component);

            //EngineLog.For<ElementInitializer>().Debug("Caller mutating script context...");
            //mutateScriptContext(context);

            EngineLog.For<ElementInitializer>().Debug("Injecting script settings into context...");
            AddSettingsToContext(context, component);

            EngineLog.For<ElementInitializer>().Debug("Plugins mutating script context...");
            AddInjectionsToContext(context, component);

            EngineLog.For<ElementInitializer>().Debug("Plugins injecting apis into script context...");
            SetupHostApisForContext(context, component);

            EngineLog.For<ElementInitializer>().Debug("Performing validation over script context...");
            ValidateContext(context, component);


            EngineLog.For<ElementInitializer>().Debug("Executing script initialization...");
            component.InitializeScript(_taskProvider, engine, source);

            EngineLog.For<ElementInitializer>().Debug("Setting up ECS element callback scene triggers...");
            if(scene == null)
            {
                EngineLog.For<ElementInitializer>().Warn("Scene not provided, can't setup callbacks. Therefore, it is the caller's responsibility to MAKE SURE TO WIRE THESE UP.");
            }
            else
            {
                component.BeforeUpdateCallback = scene.OnBeforeScriptedElementUpdate;
                component.AfterUpdateCallback = scene.OnAfterScriptedElementUpdate;
                component.BeforeDrawCallback = scene.OnBeforeScriptedElementDraw;
                component.AfterDrawCallback = scene.OnAfterScriptedElementDraw;
                component.PanicCallback = scene.OnScriptedElementPanic;
            }


            EngineLog.For<ElementInitializer>().Info("Script initialized!");
        }

        public void InitializeActorSettingBindings(ScriptedActor actor)
        {
            var context = actor.GetAttachedScriptContext();
            Dictionary<Type, BindableType> bindingInstances = new Dictionary<Type, BindableType>();

            // Load bindings defined in the module.
            foreach (var setting in actor.ModuleDeclaration.ModuleSettings)
            {
                foreach (var binding in setting.Bindings)
                {
                    EngineLog.For<SceneScriptInitializer>().Debug("Setting up binding {binding} on setting {setting}.", binding, setting);

                    if (!_bindableComponentTypes.TryGetValue(binding.TypeName, out var bindingType))
                    {
                        EngineLog.For<SceneScriptInitializer>().Warn("Binding not found on actor {actor}! Binding: {binding}.", actor.Id, binding.TypeName);
                        continue;
                    }


                    if (bindingInstances.TryGetValue(bindingType, out var instance))
                    {
                        EngineLog.For<SceneScriptInitializer>().Debug("Binding added onto existing bindable for actor {actor} from {setting} to concrete member {property} via {binding}.", actor.Id, setting.SettingName, binding.PropertyName, binding.TypeName);
                        instance.BindProperty(binding.PropertyName, setting.SettingName);
                    }
                    else
                    {
                        var newBindable = (BindableType?)Activator.CreateInstance(bindingType);
                        if (newBindable == null)
                        {
                            EngineLog.For<SceneScriptInitializer>().Warn("Binding failed instantiation for actor {actor}! Binding: {binding}.", actor.Id, binding.TypeName);
                            continue;
                        }

                        newBindable.Bind(context);
                        newBindable.BindProperty(binding.PropertyName, setting.SettingName);

                        bindingInstances.Add(bindingType, newBindable);
                        actor.Components.Add(newBindable);


                        EngineLog.For<SceneScriptInitializer>().Debug("Binding created on actor {actor} from {setting} to concrete member {property} via {binding}.", actor.Id, setting.SettingName, binding.PropertyName, binding.TypeName);
                    }
                }
            }

            // Load bindings defined in the loaded layoutdefinition/actormodules settings.
            foreach (var binding in actor.StoredDefinition.StoredBindings)
            {
                if (!_bindableComponentTypes.TryGetValue(binding.TypeName, out var bindingType))
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
            EngineLog.For<ElementInitializer>().Info("Calling " + nameof(ScriptInjectEndPoint) + " plugin end point...");
            var endPointContext = new ScriptInjectEndPoint(component.ModuleDeclaration, context);
            _pluginContext.ExecuteEndPoint<IInjectScriptContextEndPoint>(endPointContext);

            _pluginContext.WaitForEndPointExecutionAsync<IInjectScriptContextEndPoint>().WaitAndCall(component, (e, c)
                => EngineLog.For<ElementInitializer>().Error(e, "Plugin execution error on ECS element {element}!", c.Name));
        }

        private void SetupHostApisForContext(IScriptContext context, ScriptedElement component)
        {
            // Enumerate all HostAPI plugins and for each that this component uses, run the entry point.
            var hostApis = _pluginContext.GetImplementations<IHostApi>();
            foreach (var targetApi in component.ModuleDeclaration.ModuleInfo.HostApis)
            {
                EngineLog.For<ElementInitializer>().Debug("Using HostAPI {api} on ECS element {element}...", targetApi, component.Name);
                var api = hostApis.FirstOrDefault(h => h.Name == targetApi);

                if (api == null)
                {
                    EngineLog.For<ElementInitializer>().Warn("HostAPI {api} not found!", targetApi);
                    continue;
                }

                api.Use(context);
            }
        }

        private void ValidateContext(IScriptContext context, ScriptedElement component)
        {
            foreach (var declaredSetting in component.ModuleDeclaration.ModuleSettings)
            {
                if (declaredSetting.Required && !context.ContainsValue(declaredSetting.SettingName))
                {
                    EngineLog.For<ElementInitializer>().Warn("ECS element {element} is missing required module setting {setting}!", component.Name, declaredSetting.SettingName);
                    continue;
                }

                if (!declaredSetting.Required && !context.ContainsValue(declaredSetting.SettingName))
                {
                    object? value = declaredSetting.DefaultValue;

                    // Deserialize the value as appropriate for the type of setting.
                    if (declaredSetting.CachedType != null)
                    {
                        if (!declaredSetting.CachedType.TryDeserialize(declaredSetting.DefaultValue, out value, declaredSetting.SettingTypeArgs))
                        {
                            EngineLog.For<ElementInitializer>().Warn("ECS element {element} attempted to use default setting value {value} for setting {setting}, but deserialization failed! ", component.Name, value, declaredSetting.SettingName);
                            break;
                        }
                    }

                    EngineLog.For<ElementInitializer>().Debug("Added setting value ({setting} = {value}) to script context...", declaredSetting.SettingName, value);
                    context.SetValue(declaredSetting.SettingName, value);
                }
            }
        }
    }
}
