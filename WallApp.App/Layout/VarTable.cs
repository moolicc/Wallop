using System.Collections.Generic;

namespace WallApp.App.Layout
{
    class VarTable
    {
        public int Layer { get; set; }
        public Dictionary<string, object> Variables { get; private set; }


        public VarTable()
        {
            Variables = new Dictionary<string, object>();
        }

        public object this[string key]
        {
            get
            {
                return Variables[key];
            }
            set
            {
                if (Variables.ContainsKey(key))
                {
                    Variables[key] = value;
                }
                else
                {
                    Variables.Add(key, value);
                }
            }
        }

        public T GetValue<T>(string key)
        {
            return (T)Variables[key];
        }

        public bool GetValue<T>(string key, out T result)
        {
            bool success = Variables.TryGetValue(key, out object value);
            result = (T)value;
            return success;
        }

        public bool ContainsValue(string key)
        {
            return Variables.ContainsKey(key);
        }

    }
}
