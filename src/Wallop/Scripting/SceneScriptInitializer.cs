using PluginPantry;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS;
using Wallop.Shared.ECS.Serialization;
using Wallop.Shared.Modules;
using Wallop.Shared.Scripting;
using Wallop.ECS;
using Wallop.Scripting.ECS;
using Wallop.Types.Plugins;
using Wallop.Types.Plugins.EndPoints;
using System.Numerics;
using NLog.Layouts;

namespace Wallop.Scripting
{
    internal class SceneScriptInitializer
    {
        public Scene Scene { get; private set; }

        public SceneScriptInitializer(Scene scene)
        {
            Scene = scene;
        }

        public void InitializeDirectorScripts()
        {
            EngineLog.For<SceneScriptInitializer>().Info("Initializing director scripts...");
            for (int i = 0; i < Scene.Directors.Count; i++)
            {
                if (Scene.Directors[i] is ScriptedDirector scriptedDirector)
                {
                    EngineLog.For<SceneScriptInitializer>().Debug("Running director initialization for {director}...", scriptedDirector.Name);
                    if(!ECS.Serialization.ElementInitializer.Instance.InitializeElement(scriptedDirector, Scene))
                    {
                        EngineLog.For<SceneScriptInitializer>().Error("Director {director} failed to initialize!", scriptedDirector.Name);
                        EngineLog.For<SceneScriptInitializer>().Warn("Removing director {director}!", scriptedDirector.Name);
                        Scene.Directors.RemoveAt(i);
                        i--;
                    }
                }
            }
            EngineLog.For<SceneScriptInitializer>().Debug("Waiting for director script initialization to complete...");
            foreach (var director in Scene.Directors)
            {
                if (director is ScriptedDirector scriptedDirector)
                {
                    scriptedDirector.WaitForExecuteAsync().WaitAndCall(scriptedDirector, (e, d)
                        => EngineLog.For<SceneScriptInitializer>().Error(e, "Failed to initialize director script! Director: {director}, Message: {message}, Inner message: {innermessage}, Script: {script}.", d.Name, e.Message, e.InnerException?.Message, d.ModuleDeclaration.ModuleInfo.SourcePath));
                }
            }
            EngineLog.For<SceneScriptInitializer>().Info("***** Director scripts initialization complete! *****");
        }

        public void InitializeActorScripts()
        {
            foreach (var layout in Scene.Layouts)
            {
                var actors = layout.EntityRoot.GetActors<ScriptedActor>();

                EngineLog.For<SceneScriptInitializer>().Info("Initializing actor scripts for layout {layout}...", layout.Name);
                var failures = InitializeActors(actors);
                foreach (var failedActorId in failures)
                {
                    EngineLog.For<SceneScriptInitializer>().Warn("Removing actor {actor} from layout {layout}!", failedActorId, layout.Name);
                    layout.EntityRoot.RemoveFirst<ScriptedActor>(a => a.Id == failedActorId);
                }
            }
            EngineLog.For<SceneScriptInitializer>().Info("***** Actor scripts initialization complete! *****");
        }


        public string[] InitializeActors(IEnumerable<ScriptedActor> actors)
        {
            List<string> failedActors = new List<string>();
            foreach (var actor in actors)
            {
                EngineLog.For<SceneScriptInitializer>().Debug("Running actor initialization for {actor}...", actor.Id);
                if(!ECS.Serialization.ElementInitializer.Instance.InitializeElement(actor, Scene))
                {
                    EngineLog.For<SceneScriptInitializer>().Error("Actor {actor} failed to initialize!", actor.Id);
                    failedActors.Add(actor.Id);
                    continue;
                }
                
                EngineLog.For<SceneScriptInitializer>().Debug("Creating bindings for {actor}...", actor.Id);
                ECS.Serialization.ElementInitializer.Instance.InitializeActorSettingBindings(actor);
            }
            EngineLog.For<SceneScriptInitializer>().Debug("Waiting for actor script initialization to complete...");
            foreach (var actor in actors)
            {
                actor.WaitForExecuteAsync().WaitAndCall(actor, (e, a)
                    => EngineLog.For<SceneScriptInitializer>().Error(e, "Failed to initialize actor script! Actor: {actor}, Message: {message}, Inner message: {innermessage}, Script: {script}.", a.Id, e.Message, e.InnerException?.Message, a.ModuleDeclaration.ModuleInfo.SourcePath));
            }

            return failedActors.ToArray();
        }
    }
}
