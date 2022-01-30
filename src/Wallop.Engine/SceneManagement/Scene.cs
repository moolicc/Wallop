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

namespace Wallop.Engine.SceneManagement
{
    public class Scene
    {
        public List<Layout> Layouts { get; set; }

        public Layout? ActiveLayout { get; set; }

        public List<Director> Directors { get; set; }

        public PluginContext? PluginContext { get; set; }
        
        public Scene()
        {
            Layouts = new List<Layout> { };
            ActiveLayout = null;
            Directors = new List<Director> { };
        }

        public void Init(PluginContext plugins)
        {
            PluginContext = plugins;
        }

        public void BeforeScriptedActorUpdate(ScriptedEcsComponent actor)
        {
            // TODO: Find delta.
            ForMatchingHostApis(actor, (api, ctx) => api.BeforeUpdate(ctx, 0.0));
        }

        internal void Update()
        {
            if(ActiveLayout == null)
            {
                return;
            }
            foreach (var actor in ActiveLayout.EcsRoot.GetActors())
            {
                actor.Update();
            }
        }
        public void AfterScriptedActorUpdate(ScriptedEcsComponent actor)
        {
            ForMatchingHostApis(actor, (api, ctx) => api.AfterUpdate(ctx));
        }

        public void BeforeScriptedActorDraw(ScriptedEcsComponent actor)
        {
            ForMatchingHostApis(actor, (api, ctx) => api.BeforeDraw(ctx, 0.0));
        }

        internal void Draw()
        {
            if (ActiveLayout == null)
            {
                return;
            }
            foreach (var actor in ActiveLayout.EcsRoot.GetActors())
            {
                actor.Draw();
            }
        }

        public void AfterScriptedActorDraw(ScriptedEcsComponent actor)
        {
            ForMatchingHostApis(actor, (api, ctx) => api.AfterDraw(ctx));
        }

        private void ForMatchingHostApis(ScriptedEcsComponent actor, Action<IHostApi, IScriptContext> action)
        {
            var context = actor.GetAttachedScriptContext();
            var apis = PluginContext.OrThrow("PluginContext not found.").GetImplementations<IHostApi>();
            foreach (var targetApi in actor.ModuleDeclaration.ModuleInfo.HostApis)
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
                if(actor is ScriptedEcsComponent scriptedActor)
                {
                    scriptedActor.Shutdown();
                }
            }
        }
    }
}
