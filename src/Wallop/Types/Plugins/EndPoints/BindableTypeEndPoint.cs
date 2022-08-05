using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Messaging;
using Wallop.Shared.Scripting;
using Wallop.Shared.Types.Plugin;

namespace Wallop.Types.Plugins.EndPoints
{
    public class BindableTypeEndPoint : EndPointBase, IBindableTypeRegistrationEndPoint
    {
        public IEnumerable<KeyValuePair<string, Type>> BindableTypes => _bindableTypes;

        private Dictionary<string, Type> _bindableTypes;

        public BindableTypeEndPoint(Messenger messages)
            : base(messages)
        {
            _bindableTypes = new Dictionary<string, Type>();
        }

        public void Bindable<T>(string name) where T : BindableType, new()
        {
            _bindableTypes.Add(name, typeof(T));
        }
    }
}
