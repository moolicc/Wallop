using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.Scripting
{
    public class BindableComponentTypeCache
    {
        public int Count => _data.Count;

        private Dictionary<string, Type> _data;

        public BindableComponentTypeCache(IEnumerable<KeyValuePair<string, Type>> data)
        {
            _data = new Dictionary<string, Type>(data);
        }

        public bool TryGetValue(string typeName, out Type type)
        {
            return _data.TryGetValue(typeName, out type);
        }
    }
}
