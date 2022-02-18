using PluginPantry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;
using Wallop.DSLExtension.Types.Plugin;
using Wallop.Engine.ECS;
using Wallop.Engine.Scripting;
using Wallop.Engine.Scripting.ECS;

namespace Wallop.Engine.SceneManagement
{
    public class Scene
    {
        public List<Layout> Layouts { get; set; }

        public Layout? ActiveLayout { get; set; }

        public List<IDirector> Directors { get; set; }

        public PluginContext? PluginContext { get; set; }
        
        public Scene()
        {
            Layouts = new List<Layout> { };
            ActiveLayout = null;
            Directors = new List<IDirector> { };
        }

        public void Init(PluginContext plugins)
        {
            PluginContext = plugins;
        }

        public void OnBeforeScriptedElementUpdate(ScriptedElement element)
        {
            // TODO: Find delta.
            ForMatchingHostApis(element, (api, ctx) => api.BeforeUpdate(ctx, 0.0));
        }


        public void Update()
        {
            if(ActiveLayout == null)
            {
                return;
            }
            foreach (var director in Directors)
            {
                director.Update();
            }
            foreach (var actor in ActiveLayout.EcsRoot.GetActors())
            {
                actor.Update();
            }
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
            if (ActiveLayout == null)
            {
                return;
            }
            foreach (var director in Directors)
            {
                director.Draw();
            }
            foreach (var actor in ActiveLayout.EcsRoot.GetActors())
            {
                actor.Draw();
            }
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

        internal void Shutdown()
        {
            if(ActiveLayout == null)
            {
                return;
            }

            foreach (var actor in ActiveLayout.EcsRoot.GetActors())
            {
                if(actor is ScriptedElement scriptedActor)
                {
                    scriptedActor.Shutdown();
                }
            }
        }
    }
}
