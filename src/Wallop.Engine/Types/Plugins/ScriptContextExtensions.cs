using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;

namespace Wallop.Engine.Types.Plugins
{
    public static class ScriptContextExtensions
    {
        public static class Getters
        {
            public delegate GL GetGlInstance();
            public delegate Vector2 GetRenderSize();
            public delegate Vector2 GetActualSize();
            public delegate string GetBaseDirectory();
        }

        public static class Actions
        {
            public delegate void Update();
            public delegate void Draw();
        }

        public static class VariableNames
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
        }

        public static void AddProperty<TGetDelegate, TSetDelegate>(this IScriptContext context, string property, object instance, bool forceReadonly = false, bool appendAccessWords = true, bool convertLowerCamelCase = true)
            where TGetDelegate : Delegate
            where TSetDelegate : Delegate
            => AddProperty<TGetDelegate, TSetDelegate>(context, instance.GetType().GetProperty(property).OrThrow("Cannot bind ScriptContext to nonexistent property."), instance, forceReadonly, appendAccessWords, convertLowerCamelCase);

        public static void AddProperty<TGetDelegate, TSetDelegate>(this IScriptContext context, PropertyInfo property, object? instance, bool forceReadonly = false, bool appendAccessWords = true, bool convertLowerCamelCase = true)
            where TGetDelegate : Delegate
            where TSetDelegate : Delegate
        {
            var getName = property.Name;
            var setName = property.Name;
            if(appendAccessWords)
            {
                if(!getName.StartsWith("get", StringComparison.OrdinalIgnoreCase))
                {
                    getName = "get" + getName;
                }
                if (!forceReadonly && !setName.StartsWith("set", StringComparison.OrdinalIgnoreCase))
                {
                    setName = "set" + setName;
                }
            }

            if(convertLowerCamelCase)
            {
                if (char.IsUpper(getName[0]))
                {
                    getName = char.ToLower(getName[0]) + getName.Substring(1);
                }
                if (!forceReadonly && char.IsUpper(setName[0]))
                {
                    setName = char.ToLower(setName[0]) + setName.Substring(1);
                }
            }

            if(property.CanRead)
            {
                var getMethod = property.GetGetMethod().OrThrow("Property does not have get function but is readable.");
                context.AddDelegate(getName, getMethod.CreateDelegate<TGetDelegate>(instance));
            }
            if (property.CanWrite && !forceReadonly)
            {
                var setMethod = property.GetSetMethod().OrThrow("Property does not have set function but is writable.");
                context.AddDelegate(setName, setMethod.CreateDelegate<TSetDelegate>(instance));
            }
        }

        public static void AddReadonlyProperty<TGetDelegate>(this IScriptContext context, string property, object instance, bool appendAccessWord = true, bool convertLowerCamelCase = true)
            where TGetDelegate : Delegate
            => AddReadonlyProperty<TGetDelegate>(context, instance.GetType().GetProperty(property).OrThrow("Cannot bind ScriptContext to nonexistent property."), instance, appendAccessWord, convertLowerCamelCase);


        public static void AddReadonlyProperty<TGetDelegate>(this IScriptContext context, PropertyInfo property, object? instance, bool appendAccessWord = true, bool convertLowerCamelCase = true)
          where TGetDelegate : Delegate
        {
            if (!property.CanRead)
            {
                throw new InvalidOperationException("Property must be readable to bind to a context as a readonly property.");
            }

            var getName = property.Name;
            if (appendAccessWord)
            {
                if (!getName.StartsWith("get", StringComparison.OrdinalIgnoreCase))
                {
                    getName = "get" + getName;
                }
            }

            if (convertLowerCamelCase)
            {
                if (char.IsUpper(getName[0]))
                {
                    getName = char.ToLower(getName[0]) + getName.Substring(1);
                }
            }

            var getMethod = property.GetGetMethod().OrThrow("Property does not have get function but is readable.");
            context.AddDelegate(getName, getMethod.CreateDelegate<TGetDelegate>(instance));
        }

        public static GL GetGLInstance(this IScriptContext context)
            => GetDelegate<Getters.GetGlInstance>(context, VariableNames.GET_GL_INSTANCE)();

        public static Vector2 GetRenderSize(this IScriptContext context)
            => GetDelegate<Getters.GetRenderSize>(context, VariableNames.GET_RENDER_SIZE)();
        public static Vector2 GetActualSize(this IScriptContext context)
            => GetDelegate<Getters.GetActualSize>(context, VariableNames.GET_ACTUAL_SIZE)();

        public static string GetBaseDirectory(this IScriptContext context)
            => GetDelegate<Getters.GetBaseDirectory>(context, VariableNames.GET_BASE_DIRECTORY)();

        public static Actions.Update GetUpdate(this IScriptContext context)
            => GetDelegate<Actions.Update>(context, VariableNames.UPDATE);

        public static Actions.Draw GetDraw(this IScriptContext context)
            => GetDelegate<Actions.Draw>(context, VariableNames.DRAW);

        private static T Get<T>(IScriptContext context, string member)
        {
            if(!context.ContainsValue(member))
            {
                throw new KeyNotFoundException($"Script context does not contain a variable named {member}.");
            }

            var value = context.GetValue<T>(member);
            if (value == null)
            {
                throw new ArgumentNullException(member, $"Script context contains variable but it is null.");
            }

            return value;
        }

        private static T GetDelegate<T>(IScriptContext context, string member)
        {
            if (!context.ContainsDelegate(member))
            {
                throw new KeyNotFoundException($"Script context does not contain a variable named {member}.");
            }

            var value = context.GetDelegateAs<T>(member);
            if (value == null)
            {
                throw new ArgumentNullException(member, $"Script context contains variable but it is null.");
            }

            return value;
        }
    }
}
