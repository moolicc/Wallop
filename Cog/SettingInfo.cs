using Cog.Sources;

namespace Cog
{
    internal class SettingInfo
    {
        public bool IsOrphan => Source == null;
        public bool CanBeSavedToSource => Source != null && Source.CanSave;
        public string ValueLiteral { get; private set; }
        public bool IsValueResolved { get; private set; }

        public Type? ValueType => _cachedValue?.GetType() ?? null;

        public SettingBinding? Binding
        {
            get => binding;
            set
            {
                binding = value;
                if (value != null)
                {
                    if(!string.IsNullOrEmpty(ValueLiteral))
                    {
                        SetLiteralValue(ValueLiteral);
                    }
                    else if (_cachedValue != null)
                    {
                        SetValue(_cachedValue);
                    }
                }
            }
        }

        internal ISettingsSource? Source { get; set; }

        private object? _cachedValue;
        private SettingBinding? binding;

        public SettingInfo(ISettingsSource? source, string value)
        {
            Source = source;
            ValueLiteral = value;
            IsValueResolved = false;
            _cachedValue = null;
        }

        public SettingInfo(object actualValue)
            : this(null, string.Empty)
        {
            IsValueResolved = true;
            _cachedValue = actualValue;
        }

        public T? GetValue<T>()
        {
            if (Binding != null)
            {
                return Binding.GetValue<T>();
            }
            if (!IsValueResolved)
            {
                _cachedValue = Serializer.Deserialize(typeof(T), ValueLiteral);
                IsValueResolved = true;
            }
            if (_cachedValue is T result)
            {
                return result;
            }
            return default(T);
        }

        public object? GetValue()
        {
            if (Binding != null)
            {
                return Binding.GetValue<object>();
            }
            return IsValueResolved ? _cachedValue : ValueLiteral;
        }

        public void SetLiteralValue(string literalValue)
        {
            if (Binding != null)
            {
                _cachedValue = Serializer.Deserialize(Binding.ValueType, literalValue);
                Binding.SetValue(_cachedValue);
                IsValueResolved = true;
            }
            else if (IsValueResolved && _cachedValue != null)
            {
                SetValue(Serializer.Deserialize(_cachedValue.GetType(), literalValue));
            }
            else
            {
                _cachedValue = null;
                IsValueResolved = false;
            }

            ValueLiteral = literalValue;
        }

        public void SetValue(object? value)
        {
            _cachedValue = value;
            Binding?.SetValue(value?.GetType(), _cachedValue);
            IsValueResolved = true;
            ValueLiteral = string.Empty;

            if (value != null)
            {
                ValueLiteral = Serializer.Serialize(value.GetType(), value);
            }
        }
    }
}
