using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Scripting
{
    public abstract class BindableType
    {
        public bool IsBound => _boundContext != null;


        // Binds from concrete property name : setting name.
        private Dictionary<string, string> _settingBindings;

        private IScriptContext? _boundContext;

        public BindableType()
        {
            _settingBindings = new Dictionary<string, string>();
        }

        public void Bind(IScriptContext bindingContext)
        {
            if (IsBound)
            {
                // TODO: Error.
            }
            _boundContext = bindingContext;
        }

        public void BindProperty(string property, string settingName)
        {
            if (_settingBindings.ContainsKey(property))
            {
                _settingBindings[property] = settingName;
            }
            else
            {
                _settingBindings.Add(property, settingName);
            }
        }

        public IEnumerable<(string Property, string Setting)> GetBindings()
        {
            return _settingBindings.Select(kvp => (kvp.Key, kvp.Value));
        }

        public void Cleanup()
        {
            _settingBindings.Clear();
            _boundContext = null;
        }

        public bool IsSettingBound(string propertyName)
            => _settingBindings.ContainsKey(propertyName);

        protected void SetValue<T>(string propertyName, T? value, ref T? defaultValue)
        {
            defaultValue = value;
            if (_settingBindings.TryGetValue(propertyName, out var binding))
            {
                GetBindingContextSafe().SetValue(binding, value);
            }
        }

        protected T? GetValue<T>(string propertyName, ref T? defaultValue)
        {
            if (!IsBound)
            {
                return defaultValue;
            }
            if (!_settingBindings.TryGetValue(propertyName, out var scriptName))
            {
                return defaultValue;
            }
            return GetBindingContextSafe().GetValue<T>(scriptName);
        }

        private IScriptContext GetBindingContextSafe()
        {
            if (_boundContext == null)
            {
                throw new NullReferenceException("Bindable component not yet bound.");
            }
            return _boundContext;
        }
    }
}
