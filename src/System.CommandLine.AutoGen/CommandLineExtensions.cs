using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;

namespace System.CommandLine.AutoGen
{
    public static class CommandLineExtensions
    {
        public static Command Add<T>(this Command command, T model)
        {
            if(model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var type = model.GetType();
            var commandAttrib = type.GetCustomAttribute<CommandAttribute>();
            if(commandAttrib != null)
            {
                AddNewCommand(command, model, commandAttrib, false);
            }
            AddOptions(command, model);


            return command;
        }

        public static Command AddCommand<T>(this Command current, T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var commandAttrib = model.GetType().GetCustomAttribute<CommandAttribute>();
            if (commandAttrib == null)
            {
                throw new KeyNotFoundException("Provided model does not have a CommandAttribute.");
            }
            return AddNewCommand(current, model, commandAttrib, true);
        }

        public static void AddOptions<T>(this Command current, T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            var type = model.GetType();

            foreach (var property in type.GetProperties())
            {
                var optAttrib = property.GetCustomAttribute<OptionAttribute>();
                if(optAttrib == null)
                {
                    continue;
                }
                AddOption(current, model, property, optAttrib);
            }

            foreach (var field in type.GetFields())
            {
                var fldAttrib = field.GetCustomAttribute<OptionAttribute>();
                if (fldAttrib == null)
                {
                    continue;
                }
                AddOption(current, model, field, fldAttrib);
            }
        }

        public static void SetHandler<T>(this Command command, T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            var type = model.GetType();
            MethodInfo? target = null;

            foreach (var method in type.GetMethods())
            {
                var attrib = type.GetCustomAttribute<CommandHandlerAttribute>();
                if(attrib == null)
                {
                    continue;
                }
                if(target != null)
                {
                    throw new AmbiguousMatchException("Model cannot contain multiple handler methods.");
                }
                target = method;
            }
            if(target == null)
            {
                throw new InvalidOperationException("Model does not contain handler method.");
            }

            SetHandler(command, target, model);
        }

        public static void SetHandler<T>(this Command command, string methodName, T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            var type = model.GetType();
            MethodInfo? target = type.GetMethod(methodName);

            if (target == null)
            {
                throw new InvalidOperationException("Model does not contain handler method.");
            }

            SetHandler(command, target, model);
        }

        public static void SetHandler(this Command command, string staticMethodName, Type owningType)
        {
            var type = owningType;
            MethodInfo? target = type.GetMethod(staticMethodName);

            if (target == null)
            {
                throw new InvalidOperationException("Model does not contain handler method.");
            }

            SetHandler<object>(command, target, null);
        }



        private static Option AddOption<T>(Command command, T model, PropertyInfo property, OptionAttribute attribute)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            Func<object?>? defaultValueGetter = null;
            if(attribute.DefaultValueBindingType == OptionDefaultValueTimes.EarlyBound)
            {
                object? defaultValue = property.GetValue(model);
                defaultValueGetter = new Func<object?>(() => attribute.DefaultValue ?? defaultValue);
            }
            else if(attribute.DefaultValueBindingType == OptionDefaultValueTimes.LateBound)
            {
                defaultValueGetter = new Func<object?>(() => attribute.DefaultValue ?? property.GetValue(model));
            }

            var option = new Option(attribute.Aliases, attribute.Description, attribute.ArgumentType, defaultValueGetter);
            command.AddOption(option);
            return option;
        }

        private static Option AddOption<T>(Command command, T model, FieldInfo field, OptionAttribute attribute)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            Func<object?>? defaultValueGetter = null;
            if (attribute.DefaultValueBindingType == OptionDefaultValueTimes.EarlyBound)
            {
                object? defaultValue = field.GetValue(model);
                defaultValueGetter = new Func<object?>(() => attribute.DefaultValue ?? defaultValue);
            }
            else if (attribute.DefaultValueBindingType == OptionDefaultValueTimes.LateBound)
            {
                defaultValueGetter = new Func<object?>(() => attribute.DefaultValue ?? field.GetValue(model));
            }

            var option = new Option(attribute.Aliases, attribute.Description, attribute.ArgumentType, defaultValueGetter);
            command.AddOption(option);
            return option;
        }

        private static Command AddNewCommand<T>(Command parent, T model, CommandAttribute attribute, bool addOptions)
        {
            if(model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var newCmd = new Command(attribute.Name, attribute.Description);

            if(addOptions)
            {
                AddOptions(parent, model);
            }

            parent.AddCommand(newCmd);
            return newCmd;
        }

        private static void SetHandler<T>(Command command, MethodInfo handlerMethod, T? instance)
        {
        }
    }
}
