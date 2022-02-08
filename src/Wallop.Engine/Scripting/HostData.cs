using Silk.NET.OpenGL;
using System.Numerics;
using Wallop.DSLExtension.Modules;
using Wallop.DSLExtension.Scripting;
using Wallop.Engine.SceneManagement;
using Wallop.Engine.Scripting.ECS;

namespace Wallop.Engine.Scripting
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
        }


        [ScriptProperty(ExposedName = MemberNames.GET_GL_INSTANCE)]
        public GL GLInstance { get; private set; }

        public Scene Scene { get; private set; }

        public HostData(GL glInstance, Scene scene)
        {
            GLInstance = glInstance;
            Scene = scene;
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
                return new Func<Vector2>(() => Scene.ActiveLayout.OrThrow().RenderSize);
            }
            return null;
        }

        [ScriptPropertyFactory("name", FactoryPropertyMethod.Getter, ExposedName = MemberNames.GET_NAME)]
        public Func<string>? GetName(IScriptContext context, Module module, object tag)
        {
            if (tag is ScriptedEcsComponent component)
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
    }
}
