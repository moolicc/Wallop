using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Scripting
{
    internal static class ScriptContextExtensions
    {
        // GENERIC ADD PROPERTY
        public static void AddProperty<TGetDelegate, TSetDelegate>(this IScriptContext context, string property, object instance, string? alias = null, bool forceReadonly = false)
            where TGetDelegate : Delegate
            where TSetDelegate : Delegate
            => context.AddProperty<TGetDelegate, TSetDelegate>(instance.GetType().GetProperty(property), instance, alias, forceReadonly);

        public static void AddProperty<TGetDelegate, TSetDelegate>(this IScriptContext context, PropertyInfo property, object? instance, string? alias = null, bool forceReadonly = false)
            where TGetDelegate : Delegate
            where TSetDelegate : Delegate
        {

            if (property.CanRead)
            {
                var getMethod = property.GetGetMethod();

                if (getMethod == null)
                {
                    throw new MissingMethodException("Could not resolve property's Get method.");
                }

                var getName = alias ?? property.Name;
                getName = context.FormatGetterName(getName);
                context.SetDelegate(getName, getMethod.CreateDelegate<TGetDelegate>(instance));
            }
            if (property.CanWrite && !forceReadonly)
            {
                var setMethod = property.GetSetMethod();

                if (setMethod == null)
                {
                    throw new MissingMethodException("Could not resolve property's Get method.");
                }

                var setName = alias ?? property.Name;
                setName = context.FormatSetterName(setName);
                context.SetDelegate(setName, setMethod.CreateDelegate<TSetDelegate>(instance));
            }
        }



        // CONCRETE ADD PROPERTY

        public static void AddProperty(this IScriptContext context, string property, object instance, string? alias = null, bool forceReadonly = false)
            => context.AddProperty(instance.GetType().GetProperty(property), instance, alias, forceReadonly);

        public static void AddProperty(this IScriptContext context, PropertyInfo property, object? instance, string? alias = null, bool forceReadonly = false)
        {

            if (property.CanRead)
            {
                var getterType = typeof(Func<>).MakeGenericType(property.PropertyType);
                var getMethod = property.GetGetMethod();

                if (getMethod == null)
                {
                    throw new MissingMethodException("Could not resolve property's Get method.");
                }

                var getName = alias ?? property.Name;
                getName = context.FormatGetterName(getName);
                context.SetDelegate(getName, getMethod.CreateDelegate(getterType, instance));
            }
            if (property.CanWrite && !forceReadonly)
            {
                var setterType = typeof(Action<>).MakeGenericType(property.PropertyType);
                var setMethod = property.GetSetMethod();

                if (setMethod == null)
                {
                    throw new MissingMethodException("Could not resolve property's Get method.");
                }

                var setName = alias ?? property.Name;
                setName = context.FormatSetterName(setName);
                context.SetDelegate(setName, setMethod.CreateDelegate(setterType, instance));
            }
        }



        // GENERIC READONLY PROPERTY
        public static void AddReadonlyProperty<TGetDelegate>(this IScriptContext context, string property, object instance, string? alias = null)
            where TGetDelegate : Delegate
            => context.AddReadonlyProperty<TGetDelegate>(instance.GetType().GetProperty(property), instance, alias);

        public static void AddReadonlyProperty<TGetDelegate>(this IScriptContext context, PropertyInfo property, object? instance, string? alias = null)
          where TGetDelegate : Delegate
        {
            if (!property.CanRead)
            {
                throw new InvalidOperationException("Property must be readable to bind to a context as a readonly property.");
            }



            var getMethod = property.GetGetMethod();

            if (getMethod == null)
            {
                throw new MissingMethodException("Could not resolve property's Get method.");
            }

            var getName = alias ?? property.Name;
            getName = context.FormatGetterName(getName);
            context.SetDelegate(getName, getMethod.CreateDelegate<TGetDelegate>(instance));
        }


        // CONCRETE READONLY PROPERTY
        public static void AddReadonlyProperty(this IScriptContext context, string property, object instance, string? alias = null)
            => context.AddReadonlyProperty(instance.GetType().GetProperty(property), instance, alias);

        public static void AddReadonlyProperty(this IScriptContext context, PropertyInfo property, object? instance, string? alias = null)
        {
            if (!property.CanRead)
            {
                throw new InvalidOperationException("Property must be readable to bind to a context as a readonly property.");
            }


            var getMethod = property.GetGetMethod();
            var getterType = typeof(Func<>).MakeGenericType(property.PropertyType);

            if (getMethod == null)
            {
                throw new MissingMethodException("Could not resolve property's Get method.");
            }

            var getName = alias ?? property.Name;
            getName = context.FormatGetterName(getName);

            var getter = getMethod.CreateDelegate(getterType, instance);
            context.SetDelegate(getName, getter);
        }



    }
}
