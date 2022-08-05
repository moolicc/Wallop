using PluginPantry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Scripting;
using Wallop.Scripting.ECS;
using Wallop.Shared.Scripting;
using Wallop.Shared.Types.Plugin;
using Wallop.Shared.ECS;

namespace Wallop.ECS
{
    public class Scene : IScene
    {
        public string Name { get; private set; }
        public List<ILayout> Layouts { get; set; }

        public List<IDirector> Directors { get; set; }

        public PluginContext? PluginContext { get; set; }

        private ConcurrentStack<ScriptedActor> _panickedActors;
        private ConcurrentStack<ScriptedDirector> _panickedDirectors;

        public Scene(string name)
        {
            Name = name;
            Layouts = new List<ILayout>();
            Directors = new List<IDirector> { };

            _panickedActors = new ConcurrentStack<ScriptedActor>();
            _panickedDirectors = new ConcurrentStack<ScriptedDirector>();
        }

        public IEnumerable<ILayout> GetActiveLayouts()
            => Layouts.Where(l => l.IsActive);

        public void Init(PluginContext plugins)
        {
            PluginContext = plugins;
        }

        public void OnScriptedElementPanic(ScriptedElement element)
        {
            element.AfterDrawCallback = null;
            element.AfterUpdateCallback = null;
            element.BeforeDrawCallback = null;
            element.BeforeUpdateCallback = null;

            if (element is ScriptedActor actor)
            {
                _panickedActors.Push(actor);
            }
            else if (element is ScriptedDirector director)
            {
                _panickedDirectors.Push(director);
            }
        }

        public void OnBeforeScriptedElementUpdate(ScriptedElement element)
        {
            // TODO: Find delta.
            ForMatchingHostApis(element, (api, ctx) => api.BeforeUpdate(ctx, 0.0));
        }


        public void Update()
        {
            var activeLayouts = GetActiveLayouts().ToArray();
            if (activeLayouts.Length == 0)
            {
                return;
            }
            foreach (var director in Directors)
            {
                director.Update();
            }
            foreach (var director in Directors)
            {
                if (director is ScriptedElement ele)
                {
                    ele.WaitForExecuteAsync().WaitAndThrow();
                }
            }

            foreach (var layout in activeLayouts)
            {
                foreach (var actor in layout.EntityRoot.GetActors())
                {
                    actor.Update();
                }
                foreach (var actor in layout.EntityRoot.GetActors())
                {
                    if (actor is ScriptedElement ele)
                    {
                        ele.WaitForExecuteAsync().WaitAndThrow();
                    }
                }
            }
            CleanPanicked();
        }

        public void OnAfterScriptedElementUpdate(ScriptedElement element)
        {
            ForMatchingHostApis(element, (api, ctx) => api.AfterUpdate(ctx));
        }

        public void OnBeforeScriptedElementDraw(ScriptedElement element)
        {
            ForMatchingHostApis(element, (api, ctx) => api.BeforeDraw(ctx, 0.0));
        }

        public void Draw()
        {
            var activeLayouts = GetActiveLayouts().ToArray();
            if (activeLayouts.Length == 0)
            {
                return;
            }
            foreach (var director in Directors)
            {
                director.Draw();
            }
            foreach (var director in Directors)
            {
                if (director is ScriptedElement ele)
                {
                    ele.WaitForExecuteAsync().WaitAndThrow();
                }
            }

            foreach (var layout in activeLayouts)
            {
                foreach (var actor in layout.EntityRoot.GetActors())
                {
                    actor.Draw();
                }
                foreach (var actor in layout.EntityRoot.GetActors())
                {
                    if (actor is ScriptedElement ele)
                    {
                        ele.WaitForExecuteAsync().WaitAndThrow();
                    }
                }
            }
            CleanPanicked();
        }

        public void OnAfterScriptedElementDraw(ScriptedElement element)
        {
            ForMatchingHostApis(element, (api, ctx) => api.AfterDraw(ctx));
        }

        private void ForMatchingHostApis(ScriptedElement element, Action<IHostApi, IScriptContext> action)
        {
            var context = element.GetAttachedScriptContext();
            var apis = PluginContext.OrThrow("PluginContext not found.").GetImplementations<IHostApi>();
            foreach (var targetApi in element.ModuleDeclaration.ModuleInfo.HostApis)
            {
                var api = apis.FirstOrDefault(a => a.Name == targetApi);
                if (api != null)
                {
                    action(api, context);
                }
            }
        }

        private void CleanPanicked()
        {
            while (_panickedActors.TryPop(out var actor) && actor != null)
            {
                actor.OwningLayout.EntityRoot.Remove(actor);
            }
            while (_panickedDirectors.TryPop(out var director) && director != null)
            {
                Directors.Remove(director);
            }
        }

        public void Shutdown()
        {
            foreach (var item in Directors)
            {
                if (item is ScriptedDirector director)
                {
                    director.Shutdown();
                }
            }

            var activeLayouts = GetActiveLayouts().ToArray();
            if (activeLayouts.Length == 0)
            {
                return;
            }
            foreach (var layout in activeLayouts)
            {
                foreach (var actor in layout.EntityRoot.GetActors())
                {
                    if (actor is ScriptedElement scriptedActor)
                    {
                        scriptedActor.Shutdown();
                    }
                }
            }
        }

        internal string GetFriendlyString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Scene : {0}\n", Name);
            builder.AppendLine();

            foreach (var layout in Layouts)
            {
                builder.AppendFormat("  {0}Layout : {1}\n", layout.IsActive ? "*" : "", layout.Name);
                foreach (var actor in layout.EntityRoot.GetActors())
                {
                    builder.AppendFormat("    Actor : {0}\n", actor.Name);
                    if (actor is ScriptedActor sActor)
                    {
                        builder.AppendFormat("      ModuleID : {0}\n", sActor.ModuleDeclaration.ModuleInfo.Id);
                        builder.AppendFormat("      ModulePkg : {0}\n", sActor.ModuleDeclaration.ModuleInfo.PackageFile);

                        foreach (var comp in actor.Components)
                        {
                            builder.AppendFormat("      Component : {0}\n", comp.GetType().Name);
                        }
                    }
                }
            }

            return builder.ToString();
        }


    }
}
