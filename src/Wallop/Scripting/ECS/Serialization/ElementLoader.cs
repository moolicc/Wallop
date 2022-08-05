using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS;
using Wallop.Shared.ECS.Serialization;
using Wallop.Shared.Modules;
using Wallop.Shared.Scripting;

namespace Wallop.Scripting.ECS.Serialization
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

        public T Load<T>(StoredModule storedElement) where T : ScriptedElement
        {
            var type = ModuleTypes.Director;
            if(typeof(T) == typeof(ScriptedActor))
            {
                type = ModuleTypes.Actor;
            }

            EngineLog.For(nameof(ElementLoader)).Info("Loading element ({elementType}) {actor} from stored module {module}...", type, storedElement.InstanceName, storedElement.ModuleId);


            // Find the module that handles this actor.

            Module? bestCandidate = null;
            foreach (var item in _packageCache.Modules)
            {
                if(item.ModuleInfo.ScriptType != type)
                {
                    continue;
                }

                if(bestCandidate == null && item.ModuleInfo.ScriptName == storedElement.ModuleId)
                {
                    bestCandidate = item;
                }
                else if(item.ModuleInfo.Id == storedElement.ModuleId)
                {
                    bestCandidate = item;
                }
            }

            if (bestCandidate == null)
            {
                EngineLog.For(nameof(ElementLoader)).Error("Module {module} for actor definition {actor} not found!", storedElement.ModuleId, storedElement.InstanceName);
                throw new ElementLoadException("Failed to resolve module from package cache.");
            }
            
            // Adjust paths to reflect the relative nature of the package.

            if(type == ModuleTypes.Director)
            {
                return (T)(ScriptedElement)new ScriptedDirector(bestCandidate, storedElement);
            }
            return (T)(ScriptedElement)new ScriptedActor(bestCandidate, storedElement);
        }

        public IEnumerable<T> LoadAll<T>(params StoredModule[] storedElements) where T : ScriptedElement
        {
            foreach (var definition in storedElements)
            {
                yield return Load<T>(definition);
            }
        }
    }
}
