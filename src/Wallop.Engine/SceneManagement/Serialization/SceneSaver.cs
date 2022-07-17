using Wallop.DSLExtension.Modules;
using Wallop.DSLExtension.Scripting;
using Wallop.Engine.Scripting;
using Wallop.Engine.Scripting.ECS;

namespace Wallop.Engine.SceneManagement.Serialization
{
    [Flags]
    public enum SettingsSaveOptions
    {
        /// <summary>
        /// Save the state of module settings that are required.
        /// </summary>
        RequiredSettings = 0,

        /// <summary>
        /// Save the state of module settings that are NOT required.
        /// </summary>
        OptionalSettings = 1,

        /// <summary>
        /// For each module setting being saved, save the default value instead of the current value.
        /// </summary>
        UseDefaultValues = 2,

        /// <summary>
        /// Save the state of context-values that are marked as tracked.
        /// </summary>
        TrackedValues = 4,

        /// <summary>
        /// Save the state of context-values.
        /// </summary>
        EntireState = 8,

        /// <summary>
        /// Save the state of module settings that are both required, and NOT required, using the module settings' default values.
        /// </summary>
        Default = RequiredSettings | OptionalSettings | UseDefaultValues,
    }

    public class SceneSaver
    {
        public SettingsSaveOptions SettingsPolicy { get; init; }

        private PackageCache _packageCache;

        public SceneSaver(SettingsSaveOptions savePolicy, PackageCache packageCache)
        {
            SettingsPolicy = savePolicy;
            _packageCache = packageCache;
        }

        public StoredScene Save(Scene scene)
        {
            var storedScene = new StoredScene();

            storedScene.Name = scene.Name;
            storedScene.Layouts = new List<StoredLayout>(SaveLayouts(scene));
            storedScene.DirectorModules = new List<StoredModule>(SaveDirectors(scene));

            return storedScene;
        }
        public IEnumerable<StoredModule> SaveDirectors(IEnumerable<ScriptedDirector> directors)
        {
            foreach (var director in directors)
            {
                var stored = new StoredModule();

                SaveCommonElementalState(director, stored);

                yield return stored;
            }
        }

        private IEnumerable<StoredModule> SaveDirectors(Scene scene)
            => SaveDirectors(scene.Directors.Where(d => d is ScriptedDirector).Select(d => (ScriptedDirector)d));

        private IEnumerable<StoredLayout> SaveLayouts(Scene scene)
        {
            foreach (var layout in scene.Layouts)
            {
                var stored = new StoredLayout();

                stored.Name = layout.Name;
                stored.Active = scene.ActiveLayout == layout;
                stored.ActorModules = new List<StoredModule>(SaveActors(layout.EcsRoot.GetActors<ScriptedActor>()));

                yield return stored;
            }
        }

        public IEnumerable<StoredModule> SaveActors(IEnumerable<ScriptedActor> actors)
        {
            foreach (var actor in actors)
            {
                var stored = new StoredModule();

                SaveCommonElementalState(actor, stored);
                SaveActorBindings(actor, stored);

                yield return stored;
            }
        }

        private void SaveActorBindings(ScriptedActor actor, StoredModule stored)
        {
            foreach (var component in actor.Components)
            {
                if(component is not BindableType bindable)
                {
                    continue;
                }
                var type = bindable.GetType().Name;
                var boundSettings = bindable.GetBindings();

                foreach (var bound in boundSettings)
                {
                    stored.StoredBindings.Add(new StoredBinding(type, bound.Property, bound.Setting));
                }
            }
        }

        private void SaveCommonElementalState(ScriptedElement element, StoredModule stored)
        {
            stored.InstanceName = element.Name;
            stored.ModuleId = element.ModuleDeclaration.ModuleInfo.Id;


            var context = element.GetAttachedScriptContext();
            SaveElementSettings(element, stored, context);
            SaveElementContext(element, stored, context);
        }

        private void SaveElementSettings(ScriptedElement element, StoredModule stored, IScriptContext context)
        {
            bool requiredSettings = (SettingsPolicy & SettingsSaveOptions.RequiredSettings) == SettingsSaveOptions.RequiredSettings;
            bool useDefaultValues = (SettingsPolicy & SettingsSaveOptions.UseDefaultValues) == SettingsSaveOptions.UseDefaultValues;
            bool optionalSettings = (SettingsPolicy & SettingsSaveOptions.OptionalSettings) == SettingsSaveOptions.OptionalSettings;

            foreach (var setting in element.ModuleDeclaration.ModuleSettings)
            {
                if (setting.Required && requiredSettings)
                {
                    string? value = GetSettingValue(useDefaultValues, context, setting);
                    stored.Settings.Add(setting.SettingName, value);
                }
                else if (optionalSettings)
                {
                    string? value = GetSettingValue(useDefaultValues, context, setting);
                    stored.Settings.Add(setting.SettingName, value);
                }
            }
        }

        private void SaveElementContext(ScriptedElement element, StoredModule stored, IScriptContext context)
        {

            bool entireState = (SettingsPolicy & SettingsSaveOptions.EntireState) == SettingsSaveOptions.EntireState;
            bool includeTrackedValues = (SettingsPolicy & SettingsSaveOptions.TrackedValues) == SettingsSaveOptions.TrackedValues;

            var values = context.GetAddedValues();
            var trackedMembers = context.GetTrackedMembers();

            // Iterate over the entire state, if that option was selected
            // OR
            // only those members marked as tracked, if includeTrackedValues option was selected 
            foreach (var variable in values.Where(v => entireState || (includeTrackedValues && trackedMembers.Contains(v.Key))))
            {
                var type = variable.Value?.GetType();
                if(variable.Value is Delegate || type == null || type.IsAssignableTo(typeof(Delegate)))
                {
                    continue;
                }
                if(stored.Settings.ContainsSetting(variable.Key) && !includeTrackedValues)
                {
                    continue;
                }

                string? serialized = null;
                if (variable.Value != null)
                {
                    if (!_packageCache.Types.TrySerialize(variable.Value.GetType().Name, variable.Value, out serialized, null))
                    {
                        serialized = variable.Value.ToString();
                    }
                    else
                    {
                        try
                        {
                            serialized = Convert.ChangeType(variable.Value, typeof(string))?.ToString();
                        }
                        catch
                        { }
                    }
                }
                serialized ??= "";

                Type? trackedType = trackedMembers.Contains(variable.Key) ? (variable.Value?.GetType() ?? typeof(NullType)) : null;
                stored.Settings.Add(variable.Key, serialized, trackedType);
            }
        }
        private void SaveElementConfig(ScriptedElement element, StoredModule stored, IScriptContext context)
        {
            foreach (var configItem in element.Config)
            {
                stored.Config.Add(configItem.Key, configItem.Value);
            }
        }

        private string GetSettingValue(bool useDefaultValue, IScriptContext context, ModuleSetting setting)
        {
            string? value = null;
            if (useDefaultValue && setting.DefaultValue != null)
            {
                value = setting.DefaultValue;
            }
            else
            {
                var stateValue = context.GetValue(setting.SettingName);
                if(stateValue == null)
                {
                    // TODO: Error
                }
                else
                {
                    if (!_packageCache.Types.TrySerialize(setting.SettingType, stateValue, out value, setting.SettingTypeArgs))
                    {
                        // TODO: Error
                    }
                }
            }
            value ??= "";
            return value;
        }
    }
}
