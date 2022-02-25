using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.DSLExtension.Scripting;
using Wallop.Engine.SceneManagement;
using Wallop.Engine.Scripting.ECS;

namespace Wallop.Engine.Scripting
{
    [Flags]
    public enum SettingsSaveOptions
    {
        RequiredSettings = 0,
        OptionalSettings = 1,
        UseDefaultValues = 2,

        EntireState = 4,
        Default = RequiredSettings | OptionalSettings,
    }

    public class ScriptedSceneSaver
    {
        public SettingsSaveOptions SettingsPolicy { get; init; }

        private DSLExtension.Modules.SettingTypes.TypeCache _typeCache;

        public ScriptedSceneSaver(SettingsSaveOptions savePolicy)
        {
            SettingsPolicy = savePolicy;
            _typeCache = new DSLExtension.Modules.SettingTypes.TypeCache();
        }

        public StoredScene Save(Scene scene)
        {
            var storedScene = new StoredScene();

            storedScene.Name = scene.Name;
            storedScene.Layouts = new List<StoredLayout>(SaveLayouts(scene));
            storedScene.DirectorModules = new List<StoredModule>(SaveDirectors(scene));

            return storedScene;
        }

        private IEnumerable<StoredModule> SaveDirectors(Scene scene)
        {
            foreach (var director in scene.Directors)
            {
                if(director is ScriptedDirector scripted)
                {
                    var stored = new StoredModule();

                    SaveCommonElementalState(scripted, stored);

                    yield return stored;
                }
            }
        }

        private IEnumerable<StoredLayout> SaveLayouts(Scene scene)
        {
            foreach (var layout in scene.Layouts)
            {
                var stored = new StoredLayout();

                stored.Name = layout.Name;
                stored.Active = scene.ActiveLayout == layout;
                stored.ActorModules = new List<StoredModule>(SaveActors(scene, layout.EcsRoot.GetActors<ScriptedActor>()));

                yield return stored;
            }
        }

        private IEnumerable<StoredModule> SaveActors(Scene scene, IEnumerable<ScriptedActor> actors)
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
            if((SettingsPolicy & SettingsSaveOptions.EntireState) != SettingsSaveOptions.EntireState)
            {
                return;
            }
            var values = context.GetAddedValues();
            foreach (var variable in values)
            {
                var type = variable.Value?.GetType();
                if(variable.Value is Delegate || type == null || type.IsAssignableTo(typeof(Delegate)))
                {
                    continue;
                }
                if(stored.Settings.ContainsKey(variable.Key))
                {
                    continue;
                }

                string? serialized = null;
                if (variable.Value != null)
                {
                    if (!_typeCache.TrySerialize(variable.Value.GetType().Name, variable.Value, out serialized, null))
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
                stored.Settings.Add(variable.Key, serialized);
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
                    if (!_typeCache.TrySerialize(setting.SettingType, stateValue, out value, setting.SettingTypeArgs))
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
