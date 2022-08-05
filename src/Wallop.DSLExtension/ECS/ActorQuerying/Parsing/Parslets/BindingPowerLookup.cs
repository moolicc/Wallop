using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Parslets
{
    public static class BindingPowerLookup
    {

        private static Dictionary<Type, int> _table;

        static BindingPowerLookup()
        {
            _table = new Dictionary<Type, int>();
        }

        public static IEnumerable<(Type ParsletType, int BindingPower)> GetTable()
            => _table.Select(i => (i.Key, i.Value));

        public static int Get<T>()
            => Get(typeof(T));

        public static int Get(Type parsletType)
            => _table[parsletType];

        public static void Set<T>(int bindingPower)
            => Set(typeof(T), bindingPower);

        public static void Set(Type parsletType, int bindingPower)
        {
            if (!_table.TryAdd(parsletType, bindingPower))
            {
                _table[parsletType] = bindingPower;
            }
        }
    }
}
