using Silk.NET.OpenGL;
using System.Numerics;
using Wallop.ECS;
using Wallop.Scripting.ECS;
using Wallop.Shared.ECS;
using Wallop.Shared.Modules;
using Wallop.Shared.Scripting;

namespace Wallop.Scripting
{
    public class HostData
    {
        public static class ExplicitGetters
        {
            public delegate GL GetGlInstance();
            public delegate Vector2 GetRenderSize();
            public delegate Vector2 GetActualSize();
            public delegate string GetBaseDirectory();
            public delegate string GetName();
        }

        public static class Actions
        {
            public delegate void Update();
            public delegate void Draw();
        }

        public static class MemberNames
        {
            /// <summary>
            /// The name of the getter that represents the GL instance.
            /// </summary>
            public const string GET_GL_INSTANCE = "getGLInstance";

            /// <summary>
            /// The name of the getter that represents the size of the actor's layout's render target.
            /// </summary>
            public const string GET_RENDER_SIZE = "getRenderSize";

            /// <summary>
            /// The name of the getter that represents the actual, scaled size of the actor's layout's area.
            /// </summary>
            public const string GET_ACTUAL_SIZE = "getActualSize";

            /// <summary>
            /// The name of the getter that represents the actual, scaled size of the actor's layout's area.
            /// </summary>
            public const string GET_BASE_DIRECTORY = "getBaseDirectory";

            /// <summary>
            /// The name of the getter that retrieves the actor/director's logger.
            /// </summary>
            public const string GET_LOGGER = "getLogger";



            /// <summary>
            /// The name of the update function.
            /// </summary>
            public const string UPDATE = "update";

            /// <summary>
            /// The name of the draw function.
            /// </summary>
            public const string DRAW = "draw";

            /// <summary>
            /// The name of the function that retrieves the actor/director's name.
            /// </summary>
            public const string GET_NAME = "getName";

            /// <summary>
            /// The name of the trace function.
            /// </summary>
            public const string TRACE = "trace";

            /// <summary>
            /// The name of the debug function.
            /// </summary>
            public const string DEBUG = "debug";

            /// <summary>
            /// The name of the info function.
            /// </summary>
            public const string INFO = "info";

            /// <summary>
            /// The name of the warn function.
            /// </summary>
            public const string WARN = "warn";

            /// <summary>
            /// The name of the error function.
            /// </summary>
            public const string ERROR = "error";

            /// <summary>
            /// The name of the fatal function.
            /// </summary>
            public const string FATAL = "fatal";

            /// <summary>
            /// The name of the fatal function.
            /// </summary>
            public const string PANIC = "panic";

            /// <summary>
            /// The name of the function to add a tracked member.
            /// </summary>
            public const string ADD_TRACKED_MEMBER = "track"; 

            /// <summary>
            /// The name of the function to remove a tracked member.
            /// </summary>
            public const string REMOVE_TRACKED_MEMBER = "utrack";


            /// <summary>
            /// The name of the function to check if a member is in scope.
            /// </summary>
            public const string IS_DEFINED = "IsDefined";

            /// <summary>
            /// The name of the function to attempt to define a new member in scope.
            /// </summary>
            public const string DEFINE_NEW = "Define";


            /// <summary>
            /// Saves the specified member.
            /// </summary>
            public const string SAVE = "Save";

            /// <summary>
            /// Loads the specified member.
            /// </summary>
            public const string RESTORE = "Restore";

        }


        //[ScriptProperty(ExposedName = MemberNames.GET_GL_INSTANCE)]
        //public GL GLInstance { get; private set; }

        public Scene Scene { get; private set; }

        public BindableComponentTypeCache BindableComponents { get; private set; }

        private Func<GL> _glGetter;

        public HostData(Func<GL> glGetter, Scene scene, BindableComponentTypeCache bindableTypes)
        {
            //GLInstance = glGetter();
            _glGetter = glGetter;
            Scene = scene;
            BindableComponents = bindableTypes;
        }


        [ScriptFunction(typeof(Func<GL>), ExposedName = MemberNames.GET_GL_INSTANCE)]
        public GL GetGl()
        {
            return _glGetter();
        }

        [ScriptPropertyFactory("basedir", FactoryPropertyMethod.Getter, ExposedName = MemberNames.GET_BASE_DIRECTORY)]
        public Func<string>? BaseDirectory(IScriptContext context, Module module, object tag)
        {
            return new Func<string>(() =>
            {
                var fileInfo = new FileInfo(module.ModuleInfo.SourcePath);
                return fileInfo.DirectoryName.OrThrow();
            });
        }

        [ScriptPropertyFactory("rendersize", FactoryPropertyMethod.Getter, ExposedName = MemberNames.GET_RENDER_SIZE)]
        public Func<Vector2>? GetRenderSize(IScriptContext context, Module module, object tag)
        {
            if (tag is ScriptedActor actor)
            {
                return new Func<Vector2>(() => actor.OwningLayout.RenderSize);
            }
            else if(tag is ScriptedDirector director)
            {
                // FIXME
                throw new NotImplementedException();
                //return new Func<Vector2>(() => Scene.ActiveLayout.OrThrow().RenderSize);
            }
            return null;
        }

        [ScriptPropertyFactory("presentationsize", FactoryPropertyMethod.Getter, ExposedName = MemberNames.GET_ACTUAL_SIZE)]
        public Func<Vector2>? GetPresentationSize(IScriptContext context, Module module, object tag)
        {
            if (tag is ScriptedActor actor)
            {
                return new Func<Vector2>(() => new Vector2(actor.OwningLayout.PresentationBounds.X, actor.OwningLayout.PresentationBounds.Y));
            }
            else if (tag is ScriptedDirector director)
            {
                // FIXME
                //return new Func<Vector2>(() => Scene.ActiveLayout.OrThrow().PresentationSize);
            }
            return null;
        }


        [ScriptPropertyFactory("name", FactoryPropertyMethod.Getter, ExposedName = MemberNames.GET_NAME)]
        public Func<string>? GetName(IScriptContext context, Module module, object tag)
        {
            if (tag is ScriptedElement component)
            {
                return new Func<string>(() => component.Name);
            }
            return null;
        }

        [ScriptFunction(typeof(Func<string, string>))]
        public string FindActorLayout(string actorId)
        {
            if(!actorId.Contains(ScriptedActor.NAMESPACE_DELIMITER))
            {
                // TODO: ambiguity possible exception
            }
            string layoutPart = actorId.Substring(0, actorId.IndexOf(ScriptedActor.NAMESPACE_DELIMITER));
            foreach (var layout in Scene.Layouts)
            {
                if(layout.Name.Equals(layoutPart, StringComparison.OrdinalIgnoreCase))
                {
                    return layout.Name;
                }
            }
            return string.Empty;
        }

        [ScriptFunctionFactory]
        public Delegate? AddComponent(IScriptContext ctx, Module module, object? tag)
        {
            if(tag is ScriptedActor actor)
            {
                return new Action<object>(component => actor.Components.Add(component));
            }
            else if(tag is ScriptedDirector director)
            {
                return new Action<string, object>((actor, component) => FindActor(actor)?.Components.Add(component));
            }
            return null;
        }

        [ScriptFunctionFactory]
        public Delegate? AddComponentBinding(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedActor actor)
            {
                return new Action<string, object, string>((setting, componentTarget, targetProperty) =>
                {
                    if(componentTarget is BindableType bindable)
                    {
                        if(!bindable.IsBound)
                        {
                            bindable.Bind(ctx);
                        }
                        bindable.BindProperty(targetProperty, setting);
                    }
                });
            }
            else if (tag is ScriptedDirector director)
            {
                return new Action<string, string, object, string>((actor, setting, componentTarget, targetProperty) =>
                {
                    FindActor(actor);

                    if (componentTarget is BindableType bindable)
                    {
                        if (!bindable.IsBound)
                        {
                            bindable.Bind(ctx);
                        }
                        bindable.BindProperty(targetProperty, setting);
                    }
                });
            }
            return null;
        }

        [ScriptFunctionFactory]
        public Delegate? RemoveComponent(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedActor actor)
            {
                return new Action<object>(component => actor.Components.Remove(component));
            }
            else if (tag is ScriptedDirector director)
            {
                return new Action<string, object>((actor, component) => FindActor(actor)?.Components.Remove(component));
            }
            return null;
        }

        [ScriptFunctionFactory]
        public Delegate? GetComponents(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedActor actor)
            {
                return new Func<IEnumerable<object>>(() => actor.Components.ToArray());
            }
            else if (tag is ScriptedDirector director)
            {
                return new Func<string, IEnumerable<object>>(actor => FindActor(actor)?.Components.ToArray() ?? Array.Empty<object>());
            }
            return null;
        }


        [ScriptFunctionFactory]
        public Delegate? GetComponent(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedActor actor)
            {
                return new Func<Predicate<object>, object?>((selector) => actor.Components.FirstOrDefault(selector));
            }
            else if (tag is ScriptedDirector director)
            {
                return new Func<string, Predicate<object>, object?>((actor, selector) => FindActor(actor)?.Components.FirstOrDefault(selector));
            }
            return null;
        }

        [ScriptFunctionFactory]
        public Delegate? GetComponentByName(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedActor actor)
            {
                return new Func<Predicate<object>, object?>((selector) => actor.Components.FirstOrDefault(selector));
            }
            else if (tag is ScriptedDirector director)
            {
                return new Func<string, string, object?>((actor, componentName) =>
                {
                    return FindActor(actor)?.Components.FirstOrDefault(c => c.GetType().Name == componentName);
                });
            }
            return null;
        }

        [ScriptFunctionFactory]
        public Delegate? FindActors(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedActor actor)
            {
                return new Func<string, IEnumerable<ScriptedActor>>(query => actor.OwningLayout.EntityRoot.GetActors<ScriptedActor>(query));
            }
            else if (tag is ScriptedDirector director)
            {
                return new Func<string, IEnumerable<ScriptedActor>>(query =>
                {
                    var actors = new List<ScriptedActor>();
                    foreach (var layout in Scene.Layouts)
                    {
                        actors.AddRange(layout.EntityRoot.GetActors<ScriptedActor>(query));
                    }
                    return actors.ToArray();
                });
            }
            return null;
        }




        [ScriptFunctionFactory(ExposedName = MemberNames.TRACE)]
        public Delegate? Trace(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Action<string>(msg => ModuleLog.For(module, element.Name).Trace(msg));
            }
            return null;
        }

        [ScriptFunctionFactory(ExposedName = MemberNames.DEBUG)]
        public Delegate? Debug(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Action<string>(msg => ModuleLog.For(module, element.Name).Debug(msg));
            }
            return null;
        }

        [ScriptFunctionFactory(ExposedName = MemberNames.INFO)]
        public Delegate? Info(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Action<string>(msg => ModuleLog.For(module, element.Name).Info(msg));
            }
            return null;
        }

        [ScriptFunctionFactory(ExposedName = MemberNames.WARN)]
        public Delegate? Warn(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Action<string>(msg => ModuleLog.For(module, element.Name).Warn(msg));
            }
            return null;
        }

        [ScriptFunctionFactory(ExposedName = MemberNames.ERROR)]
        public Delegate? Error(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Action<string>(msg => ModuleLog.For(module, element.Name).Error(msg));
            }
            return null;
        }

        [ScriptFunctionFactory(ExposedName = MemberNames.FATAL)]
        public Delegate? Fatal(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Action<string>(msg => ModuleLog.For(module, element.Name).Trace(msg));
            }
            return null;
        }

        [ScriptPropertyFactory("logger", FactoryPropertyMethod.Getter, ExposedName = MemberNames.GET_LOGGER)]
        public Delegate? Logger(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Func<NLog.Logger>(() => ModuleLog.For(module, element.Name));
            }
            return null;
        }


        [ScriptFunctionFactory(ExposedName = MemberNames.PANIC)]
        public Delegate? Panic(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Action<string>(reason => element.Panic(reason, true));
            }
            return null;
        }


        [ScriptFunctionFactory(ExposedName = MemberNames.ADD_TRACKED_MEMBER)]
        public Delegate? Track(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Action<string>(member => ctx.SetTrackedMember(member));
            }
            return null;
        }

        [ScriptFunctionFactory(ExposedName = MemberNames.REMOVE_TRACKED_MEMBER)]
        public Delegate? Untrack(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Action<string>(member => ctx.SetTrackedMember(member, false));
            }
            return null;
        }

        [ScriptFunctionFactory(ExposedName = MemberNames.IS_DEFINED)]
        public Delegate? IsDefined(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Func<string, bool>(member => ctx.ContainsValue(member));
            }
            return null;
        }

        [ScriptFunctionFactory(ExposedName = MemberNames.DEFINE_NEW)]
        public Delegate? DefineNew(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Func<string, object?, bool>((member, value) =>
                {
                    if(!ctx.ContainsValue(member))
                    {
                        return true;
                    }
                    ctx.SetValue(member, value);
                    return true;
                });
            }
            return null;
        }

        [ScriptFunctionFactory(ExposedName = MemberNames.SAVE)]
        public Delegate? Save(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Action<string, string>((key, value) => element.Config.Add(key, value));
            }
            return null;
        }

        [ScriptFunctionFactory(ExposedName = MemberNames.RESTORE)]
        public Delegate? Restore(IScriptContext ctx, Module module, object? tag)
        {
            if (tag is ScriptedElement element)
            {
                return new Func<string, string>((key) => element.Config[key]);
            }
            return null;
        }


        private ScriptedActor? FindActor(string actorId)
        {
            // FIXME

            return null;
        }

    }
}
