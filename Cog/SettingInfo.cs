using Cog.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cog
{
    internal class SettingInfo
    {
        public bool IsOrphan => Source == null;
        public bool CanBeSavedToSource => Source != null && Source.CanSave;

        public Type ValueType { get; private set; }

        public SettingBinding? Binding { get; set; }

        internal ISettingsSource? Source { get; set; }

        private object _value;


        public SettingInfo(ISettingsSource? source, object value)
        {
            Source = source;
            _value = value;
            ValueType = value.GetType();
        }

        public T GetValue<T>()
        {
            if(Binding != null)
            {
                return Binding.GetValue<T>();
            }
            return (T)_value;
        }

        public object GetValue()
        {
            if(Binding != null)
            {
                return Binding.GetValue<object>();
            }
            return _value;
        }

#pragma warning disable CS8601 // Possible null reference assignment.
        public void SetValue<T>(T value)
        {
            Binding?.SetValue(value);
            _value = value;
        }
#pragma warning restore CS8601 // Possible null reference assignment.
    }
}
