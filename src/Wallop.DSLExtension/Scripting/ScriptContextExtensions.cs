using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Scripting
{
    internal static class ScriptContextExtensions
    {
        // GENERIC ADD PROPERTY
        public static void AddProperty<TGetDelegate, TSetDelegate>(this IScriptContext context, string property, object instance, string? alias = null, bool forceReadonly = false, bool appendAccessWords = true, bool convertLowerCamelCase = true)
            where TGetDelegate : Delegate
            where TSetDelegate : Delegate
            => AddProperty<TGetDelegate, TSetDelegate>(context, instance.GetType().GetProperty(property), instance, alias, forceReadonly, appendAccessWords, convertLowerCamelCase);

        public static void AddProperty<TGetDelegate, TSetDelegate>(this IScriptContext context, PropertyInfo property, object? instance, string? alias = null, bool forceReadonly = false, bool appendAccessWords = true, bool convertLowerCamelCase = true)
            where TGetDelegate : Delegate
            where TSetDelegate : Delegate
        {
            var getName = alias ?? property.Name;
            var setName = alias ?? property.Name;

            if (appendAccessWords)
            {
                if (!getName.StartsWith("get", StringComparison.OrdinalIgnoreCase))
                {
                    getName = "get" + getName;
                }
                if (!forceReadonly && !setName.StartsWith("set", StringComparison.OrdinalIgnoreCase))
                {
                    setName = "set" + setName;
                }
            }

            if (convertLowerCamelCase)
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

            if (property.CanRead)
            {
                var getMethod = property.GetGetMethod();
                context.SetDelegate(getName, getMethod.CreateDelegate<TGetDelegate>(instance));
            }
            if (property.CanWrite && !forceReadonly)
            {
                var setMethod = property.GetSetMethod();
                context.SetDelegate(setName, setMethod.CreateDelegate<TSetDelegate>(instance));
            }
        }



        // CONCRETE ADD PROPERTY

        public static void AddProperty(this IScriptContext context, string property, object instance, string? alias = null, bool forceReadonly = false, bool appendAccessWords = true, bool convertLowerCamelCase = true)
            => AddProperty(context, instance.GetType().GetProperty(property), instance, alias, forceReadonly, appendAccessWords, convertLowerCamelCase);

        public static void AddProperty(this IScriptContext context, PropertyInfo property, object? instance, string? alias = null, bool forceReadonly = false, bool appendAccessWords = true, bool convertLowerCamelCase = true)
        {
            var getName = alias ?? property.Name;
            var setName = alias ?? property.Name;

            if (appendAccessWords)
            {
                if (!getName.StartsWith("get", StringComparison.OrdinalIgnoreCase))
                {
                    getName = "get" + getName;
                }
                if (!forceReadonly && !setName.StartsWith("set", StringComparison.OrdinalIgnoreCase))
                {
                    setName = "set" + setName;
                }
            }

            if (convertLowerCamelCase)
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

            if (property.CanRead)
            {
                var getterType = typeof(Func<>).MakeGenericType(property.PropertyType);
                var getMethod = property.GetGetMethod();
                context.SetDelegate(getName, getMethod.CreateDelegate(getterType, instance));
            }
            if (property.CanWrite && !forceReadonly)
            {
                var setterType = typeof(Action<>).MakeGenericType(property.PropertyType);
                var setMethod = property.GetSetMethod();
                context.SetDelegate(setName, setMethod.CreateDelegate(setterType, instance));
            }
        }



        // GENERIC READONLY PROPERTY
        public static void AddReadonlyProperty<TGetDelegate>(this IScriptContext context, string property, object instance, string? alias = null, bool appendAccessWord = true, bool convertLowerCamelCase = true)
            where TGetDelegate : Delegate
            => AddReadonlyProperty<TGetDelegate>(context, instance.GetType().GetProperty(property), instance, alias, appendAccessWord, convertLowerCamelCase);

        public static void AddReadonlyProperty<TGetDelegate>(this IScriptContext context, PropertyInfo property, object? instance, string? alias = null, bool appendAccessWord = true, bool convertLowerCamelCase = true)
          where TGetDelegate : Delegate
        {
            if (!property.CanRead)
            {
                throw new InvalidOperationException("Property must be readable to bind to a context as a readonly property.");
            }

            var getName = alias ?? property.Name;
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

            var getMethod = property.GetGetMethod();
            context.SetDelegate(getName, getMethod.CreateDelegate<TGetDelegate>(instance));
        }


        // CONCRETE READONLY PROPERTY
        public static void AddReadonlyProperty(this IScriptContext context, string property, object instance, string? alias = null, bool appendAccessWord = true, bool convertLowerCamelCase = true)
            => AddReadonlyProperty(context, instance.GetType().GetProperty(property), instance, alias, appendAccessWord, convertLowerCamelCase);

        public static void AddReadonlyProperty(this IScriptContext context, PropertyInfo property, object? instance, string? alias = null, bool appendAccessWord = true, bool convertLowerCamelCase = true)
        {
            if (!property.CanRead)
            {
                throw new InvalidOperationException("Property must be readable to bind to a context as a readonly property.");
            }

            var getName = alias ?? property.Name;
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

            var getMethod = property.GetGetMethod();

            var getterType = typeof(Func<>).MakeGenericType(property.PropertyType);
            var getter = getMethod.CreateDelegate(getterType, instance);
            context.SetDelegate(getName, getter);
        }



    }
}
