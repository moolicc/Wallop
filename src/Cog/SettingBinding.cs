using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Cog
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
    internal class SettingBinding
    {
        public object Instance { get; private set; }

        public Type ValueType => _property?.PropertyType ?? _field.FieldType;
        
        private PropertyInfo _property;
        private FieldInfo _field;


        public SettingBinding(object instance, PropertyInfo property)
        {
            Instance = instance;
            _property = property;
        }

        public SettingBinding(object instance, FieldInfo field)
        {
            Instance = instance;
            _field = field;
        }

        public T GetValue<T>()
        {
            if(_property != null )
            {
                return (T)_property.GetValue(Instance);
            }
            if(_field != null)
            {
                return (T)_field.GetValue(Instance);
            }
            throw new NullReferenceException("Setting has no member associated with it.");
        }

        public void SetValue<T>(T value)
        {
            if (_property != null)
            {
                _property.SetValue(Instance, value);
            }
            else if (_field != null)
            {
                _field.SetValue(Instance, value);
            }
            else
            {
                throw new NullReferenceException("Setting has no member associated with it.");
            }
        }

        public void SetValue(Type? type, object? value)
        {
            if(type == null)
            {
                type = ValueType;
            }
            if (_property != null)
            {
                _property.SetValue(Instance, value);
            }
            if (_field != null)
            {
                _field.SetValue(Instance, value);
            }
        }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8603 // Possible null reference return.
}
