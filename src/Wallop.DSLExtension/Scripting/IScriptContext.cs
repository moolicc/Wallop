using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Scripting
{
    public record Import(string Assembly, string Namespace);
    public record ExposedValue(string MemberName, object? Value);
    public record ExposedDelegate(string MemberName, Delegate Action);

    public interface IScriptContext
    {
        public bool ContainsValue(string name);
        public void SetValue(string name, object? value);
        public void SetValue(ExposedValue valueInfo);

        public object? GetValue(string name);
        public T? GetValue<T>(string name);

        public T GetDelegateAs<T>(string memberName);

        public void AddReference(Assembly assembly);
        //public void AddReference(string assembly);

        public void AddImport(string ns);
        public void AddImport(Import import);

        bool ContainsDelegate(string name);
        public void SetDelegate(string name, Delegate method);
        public void SetDelegate(ExposedDelegate exposedDelegate);
    }
}
