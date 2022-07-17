using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Modules;
using Wallop.DSLExtension.Scripting;
using Module = Wallop.DSLExtension.Modules.Module;

namespace Wallop.DSLExtension.Scripting
{
    public delegate Delegate? Factory(IScriptContext ctx, Module module, object? tag);

    /// <summary>
    /// Provides dependency injection into scripts.
    /// </summary>
    public class ScriptHostFunctions
    {       

        private record struct ScannedMethod(MethodInfo Method, Type BackingDelegate, object? Instance, string Name);
        private record struct ScannedProperty(PropertyInfo Property, object? Instance, string Name, bool Readable, bool Writable);
        private record struct ScannedField(FieldInfo Field, object? Instance, string Name);
        private record struct ExposedMethod(Delegate Action, string Name);
        private record struct ExposedProperty(Delegate? Getter, Delegate? Setter, string Name);

        private record struct FactoryMethod(Factory Factory, string Name);
        private record struct FactoryProperty(Factory? Getter, Factory? Setter, string Name);



        private List<ScannedMethod> _actions;
        private List<ScannedProperty> _properties;
        private List<ScannedField> _fields;

        private List<ExposedMethod> _addedMethods;
        private List<ExposedProperty> _addedProperties;

        // TODO: Add attributes for factories that can be scanned
        private List<FactoryMethod> _factoryMethods;
        private List<FactoryProperty> _factoryProperties;

        public ScriptHostFunctions()
        {
            _actions = new List<ScannedMethod>();
            _properties = new List<ScannedProperty>();
            _fields = new List<ScannedField>();

            _addedMethods = new List<ExposedMethod>();
            _addedProperties = new List<ExposedProperty>();

            _factoryMethods = new List<FactoryMethod>();
            _factoryProperties = new List<FactoryProperty>();
        }

        public void AddDependencies<T>(T instance)
        {
            var type = typeof(T);
            ScanMethods(instance, type);
            ScanProperties(instance, type);
            ScanFields(instance, type);
        }

        public void AddDependencyProperty<T>(string name, Func<T> getter)
        {
            _addedProperties.Add(new ExposedProperty(getter, null, name));
        }

        public void AddDependencyProperty<T>(string name, Func<T> getter, Action<T> setter)
        {
            _addedProperties.Add(new ExposedProperty(getter, setter, name));
        }

        public void AddDependencyMethod(string name, Delegate action)
        {
            _addedMethods.Add(new ExposedMethod(action, name));
        }

        // TODO: Add factories for properties
        public void AddDependencyMethod(string name, Factory factory)
        {
            _factoryMethods.Add(new FactoryMethod(factory, name));
        }

        public void AddDependencyProperty(string name, Factory? getterFactory, Factory? setterFactory)
        {
            _factoryProperties.Add(new FactoryProperty(getterFactory, setterFactory, name));
        }

        public virtual void Inject(IScriptContext context, Module onBehalfOf, object? tag)
        {
            // Scanned
            //===============================================

            foreach (var action in _actions)
            {
                var name = action.Name;
                name = context.FormatFunctionName(name);
                context.SetDelegate(name, action.Method.CreateDelegate(action.BackingDelegate, action.Instance));
            }

            foreach (var property in _properties)
            {
                if(!property.Writable && property.Readable)
                {
                    context.AddReadonlyProperty(property.Property, property.Instance, property.Name);
                }
                else if(property.Writable && property.Readable)
                {
                    context.AddProperty(property.Property, property.Instance, property.Name, false);
                }
            }

            foreach (var field in _fields)
            {
                // FIXME
                throw new NotImplementedException();
            }


            // Added
            //===============================================

            foreach (var action in _addedMethods)
            {
                var name = action.Name;
                name = context.FormatFunctionName(name);
                context.SetDelegate(name, action.Action);
            }

            foreach (var property in _addedProperties)
            {
                if (property.Getter != null)
                {
                    var name = property.Name;
                    name = context.FormatGetterName(name);
                    context.SetDelegate(name, property.Getter);
                }
                if(property.Setter != null)
                {
                    var name = property.Name;
                    name = context.FormatSetterName(name);
                    context.SetDelegate(name, property.Setter);
                }
            }


            // Factories
            //===============================================

            foreach (var factory in _factoryMethods)
            {
                var method = factory.Factory(context, onBehalfOf, tag);
                if(method != null)
                {
                    var name = context.FormatFunctionName(factory.Name);
                    context.SetDelegate(name, method);
                }
            }

            foreach (var factory in _factoryProperties)
            {
                var getter = factory.Getter?.Invoke(context, onBehalfOf, tag);
                var setter = factory.Setter?.Invoke(context, onBehalfOf, tag);

                if(getter != null)
                {
                    var name = factory.Name;
                    name = context.FormatGetterName(name);
                    context.SetDelegate(name, getter);
                }
                if (setter != null)
                {
                    var name = factory.Name;
                    name = context.FormatSetterName(name);
                    context.SetDelegate(name, setter);
                }
            }
        }

        private void ScanMethods<T>(T instance, Type instanceType)
        {
            foreach (var method in instanceType.GetMethods())
            {
                if (TryScanFunction(instance, method, out var scannedMethod))
                {
                    _actions.Add(scannedMethod);
                }
                else if (TryScanFunctionFactory(instance, method, out var factoryMethod))
                {
                    _factoryMethods.Add(factoryMethod);
                }
                else if (TryScanPropertyMethod(instance, method, out var propertyMethod))
                {
                    _addedProperties.Add(propertyMethod);
                }
                else if(TryScanPropertyFactoryMethod(instance, instanceType, method, out var propertyFactoryMethod))
                {
                    _factoryProperties.Add(propertyFactoryMethod);
                }
            }
        }

        private void ScanProperties<T>(T instance, Type instanceType)
        {
            foreach (var property in instanceType.GetProperties())
            {
                var attrib = property.GetCustomAttribute<ScriptPropertyAttribute>();
                if (attrib == null)
                {
                    continue;
                }
                var name = attrib.ExposedName ?? property.Name;
                var writable = (attrib.Writable ?? true) && property.CanWrite && property.GetSetMethod() != null;
                var readable = (attrib.Readable ?? true) && property.CanRead && property.GetGetMethod() != null;

                _properties.Add(new ScannedProperty(property, instance, name, readable, writable));
            }
        }

        private void ScanFields<T>(T instance, Type instanceType)
        {
            foreach (var field in instanceType.GetFields())
            {
                var attrib = field.GetCustomAttribute<ScriptPropertyAttribute>();
                if (attrib == null)
                {
                    continue;
                }
                var name = attrib.ExposedName ?? field.Name;

                _fields.Add(new ScannedField(field, instance, name));
            }
        }

        private bool TryScanFunction(object? instance, MethodInfo method, out ScannedMethod result)
        {
            result = new ScannedMethod();

            var attrib = method.GetCustomAttribute<ScriptFunctionAttribute>();
            if(attrib == null)
            {
                return false;
            }

            var name = attrib.ExposedName ?? method.Name;
            result = new ScannedMethod(method, attrib.BackingDelegateType, instance, name);
            return true;
        }

        private bool TryScanFunctionFactory(object? instance, MethodInfo method, out FactoryMethod result)
        {
            result = new FactoryMethod();

            var factoryAttrib = method.GetCustomAttribute<ScriptFunctionFactoryAttribute>();
            if(factoryAttrib == null)
            {
                return false;
            }
            if (!method.ReturnType.IsAssignableFrom(typeof(Delegate)))
            {
                // TODO: Error;
                return false;
            }
            var parameters = method.GetParameters();
            if (parameters.Length != 3)
            {
                // TODO: Error;
                return false;
            }

            var name = factoryAttrib.ExposedName ?? method.Name;
            var factory = method.CreateDelegate<Factory>(instance);
            result = new FactoryMethod(factory, name);

            return true;
        }

        private bool TryScanPropertyMethod(object? instance, MethodInfo method, out ExposedProperty result)
        {
            result = new ExposedProperty();

            var propAttrib = method.GetCustomAttribute<ScriptPropertyAttribute>();
            if (propAttrib == null)
            {
                return false;
            }

            var propName = propAttrib.ExposedName ?? method.Name;
            if (method.ReturnType == typeof(void))
            {
                // Assume it's a setter with a single parameter.
                if (method.GetParameters().Length != 1)
                {
                    // TODO: Error;
                    return false;
                }

                var actionType = typeof(Action<>).MakeGenericType(method.GetParameters()[0].ParameterType);
                var setter = method.CreateDelegate(actionType, instance);
                result = new ExposedProperty(null, setter, propName);
            }
            else
            {
                // Assume it's a getter without any parameters.
                if (method.GetParameters().Length != 0)
                {
                    // TODO: Error;
                    return false;
                }

                var funcType = typeof(Func<>).MakeGenericType(method.ReturnType);
                var getter = method.CreateDelegate(funcType, instance);
                result = new ExposedProperty(getter, null, propName);
            }

            return true;
        }


        private bool TryScanPropertyFactoryMethod(object? instance, Type instanceType, MethodInfo method, out FactoryProperty result)
        {
            result = new FactoryProperty();

            var propAttrib = method.GetCustomAttribute<ScriptPropertyFactoryAttribute>();
            if (propAttrib == null)
            {
                return false;
            }

            if (!method.ReturnType.IsAssignableTo(typeof(Delegate)))
            {
                // TODO: Error
                return false;
            }

            var name = propAttrib.ExposedName ?? method.Name;
            Factory? getter = null;
            Factory? setter = null;

            if (propAttrib.Accessor == FactoryPropertyMethod.Getter)
            {
                getter = method.CreateDelegate<Factory>(instance);
                setter = FindPropertyPal(instance, instanceType, FactoryPropertyMethod.Setter, propAttrib.PropertyId);
            }
            else
            {
                getter = FindPropertyPal(instance, instanceType, FactoryPropertyMethod.Getter, propAttrib.PropertyId);
                setter = method.CreateDelegate<Factory>(instance);
            }

            result = new FactoryProperty(getter, setter, name);
            return true;
        }

        private Factory? FindPropertyPal(object? instance, Type owningType, FactoryPropertyMethod targetAccessor, string id)
        {
            var method = owningType.GetMethods().FirstOrDefault(m =>
            {
                var attrib = m.GetCustomAttribute<ScriptPropertyFactoryAttribute>();
                if(attrib == null)
                {
                    return false;
                }
                if(attrib.PropertyId != id)
                {
                    return false;
                }
                if(attrib.Accessor != targetAccessor)
                {
                    return false;
                }
                return true;
            });

            return method?.CreateDelegate<Factory>(instance);
        }
    }
}
