using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Scripting;

namespace Wallop.Shared.Types.Plugin
{
    public interface IBindableTypeRegistrationEndPoint
    {
        public IEnumerable<KeyValuePair<string, Type>> BindableTypes { get; }

        public void Bindable<T>(string name) where T : BindableType, new();
    }
}
