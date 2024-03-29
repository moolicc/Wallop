﻿using PluginPantry;
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
            foreach (var director in Scene.Directors)
            {
                if(director is ScriptedDirector scriptedDirector)
                {
                    EngineLog.For<SceneScriptInitializer>().Debug("Running director initialization for {director}...", director.Name);
                    ECS.Serialization.ElementInitializer.Instance.InitializeElement(scriptedDirector, Scene);
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
                InitializeActors(actors);
            }
            EngineLog.For<SceneScriptInitializer>().Info("***** Actor scripts initialization complete! *****");
        }


        public void InitializeActors(IEnumerable<ScriptedActor> actors)
        {
            foreach (var actor in actors)
            {
                EngineLog.For<SceneScriptInitializer>().Debug("Running actor initialization for {actor}...", actor.Id);
                ECS.Serialization.ElementInitializer.Instance.InitializeElement(actor, Scene);
                EngineLog.For<SceneScriptInitializer>().Debug("Creating bindings for {actor}...", actor.Id);
                ECS.Serialization.ElementInitializer.Instance.InitializeActorSettingBindings(actor);
            }
            EngineLog.For<SceneScriptInitializer>().Debug("Waiting for actor script initialization to complete...");
            foreach (var actor in actors)
            {
                actor.WaitForExecuteAsync().WaitAndCall(actor, (e, a)
                    => EngineLog.For<SceneScriptInitializer>().Error(e, "Failed to initialize actor script! Actor: {actor}, Message: {message}, Inner message: {innermessage}, Script: {script}.", a.Id, e.Message, e.InnerException?.Message, a.ModuleDeclaration.ModuleInfo.SourcePath));
            }
        }
    }
}
