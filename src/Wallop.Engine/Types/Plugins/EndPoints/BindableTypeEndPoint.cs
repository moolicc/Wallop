using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;

namespace Wallop.Engine.Types.Plugins.EndPoints
{
    public class BindableTypeEndPoint : EndPointBase, DSLExtension.Types.Plugin.IBindableTypeRegistrationEndPoint
    {
        public IEnumerable<KeyValuePair<string, Type>> BindableTypes => _bindableTypes;

        private Dictionary<string, Type> _bindableTypes;

        public BindableTypeEndPoint()
        {
            _bindableTypes = new Dictionary<string, Type>();
        }

        public void Bindable<T>(string name) where T : BindableType, new()
        {
            _bindableTypes.Add(name, typeof(T));
        }
    }
}
