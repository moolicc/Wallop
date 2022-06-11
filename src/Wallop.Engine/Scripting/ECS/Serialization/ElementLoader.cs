using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;

namespace Wallop.Engine.Scripting.ECS.Serialization
{
    public class ElementLoader
    {
        public static ElementLoader Instance
        {
            get
            {
                if(_instance == null)
                {
                    throw new InvalidOperationException("ElementLoader not yet initialized.");
                }
                return _instance;
            }
        }

        private static ElementLoader? _instance;


        public PackageCache _packageCache;

        public ElementLoader(PackageCache packageCache)
        {
            if (_instance != null)
            {
                EngineLog.For<ElementLoader>().Warn("Double instantiation detected. Reference to previous instance will be forgotten.");
            }
            _instance = this;
            _packageCache = packageCache;
        }

        public T Load<T>(SceneManagement.StoredModule storedElement) where T : ScriptedElement
        {
            var type = ModuleTypes.Director;
            if(typeof(T) == typeof(ScriptedActor))
            {
                type = ModuleTypes.Actor;
            }

            EngineLog.For(nameof(ElementLoader)).Info("Loading element ({elementType}) {actor} from stored module {module}...", type, storedElement.InstanceName, storedElement.ModuleId);


            // Find the module that handles this actor.
            var associatedModule = _packageCache.Modules.FirstOrDefault(
                m => m.ModuleInfo.ScriptType == type
                && m.ModuleInfo.Id == storedElement.ModuleId);

            if (associatedModule == null)
            {
                EngineLog.For(nameof(ElementLoader)).Error("Module {module} for actor definition {actor} not found!", storedElement.ModuleId, storedElement.InstanceName);
                throw new InvalidOperationException(""); //TODO
            }

            if(type == ModuleTypes.Director)
            {
                return (T)(ScriptedElement)new ScriptedDirector(associatedModule, storedElement);
            }
            return (T)(ScriptedElement)new ScriptedActor(associatedModule, storedElement);
        }

        public IEnumerable<T> LoadAll<T>(params SceneManagement.StoredModule[] storedElements) where T : ScriptedElement
        {
            foreach (var definition in storedElements)
            {
                yield return Load<T>(definition);
            }
        }
    }
}
