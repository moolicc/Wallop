using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;
using static Wallop.Engine.Scripting.HostData;

namespace Wallop.Engine.Scripting
{
    public static class ScriptContextExtensions
    {
        public static GL GetGLInstance(this IScriptContext context)
            => GetDelegate<Func<GL>>(context, MemberNames.GET_GL_INSTANCE)();

        public static Vector2 GetRenderSize(this IScriptContext context)
            => GetDelegate<Func<Vector2>>(context, MemberNames.GET_RENDER_SIZE)();
        public static Vector2 GetActualSize(this IScriptContext context)
            => GetDelegate<ExplicitGetters.GetActualSize>(context, MemberNames.GET_ACTUAL_SIZE)();

        public static string GetBaseDirectory(this IScriptContext context)
            => GetDelegate<Func<string>>(context, MemberNames.GET_BASE_DIRECTORY)();

        public static Actions.Update GetUpdate(this IScriptContext context)
            => GetDelegate<Actions.Update>(context, MemberNames.UPDATE);

        public static Actions.Draw GetDraw(this IScriptContext context)
            => GetDelegate<Actions.Draw>(context, MemberNames.DRAW);

        public static string GetName(this IScriptContext context)
            => GetDelegate<Func<string>>(context, MemberNames.GET_NAME)();

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
