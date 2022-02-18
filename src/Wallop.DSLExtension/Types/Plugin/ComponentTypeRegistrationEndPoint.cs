using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Types.Plugin
{
    public interface IBindableTypeRegistrationEndPoint
    {
        public IEnumerable<KeyValuePair<string, Type>> BindableTypes { get; }

        public void Bindable<T>(string name) where T : Scripting.BindableType, new();
    }
}
