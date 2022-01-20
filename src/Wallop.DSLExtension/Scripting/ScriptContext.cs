using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.DSLExtension.Scripting
{
    public record Import(string Assembly, string Namespace);
    public record ExposedValue(string MemberName, object? Value);
    public record ExposedDelegate(string MemberName, Delegate Action);

    public class ScriptContext
    {
        public List<string> References { get; private set; }
        public List<Import> Imports { get; private set; }
        public List<ExposedValue> ExposedVariables { get; private set; }
        public List<ExposedDelegate> ExposedDelegates { get; private set; }

        public ScriptContext()
        {
            References = new List<string>();
            Imports = new List<Import>();
            ExposedVariables = new List<ExposedValue>();
            ExposedDelegates = new List<ExposedDelegate>();
        }

        public ScriptContext AddReference(string reference)
        {
            References.Add(reference);
            return this;
        }


        public ScriptContext AddImport(string assembly, string ns)
            => AddImport(new Import(assembly, ns));

        public ScriptContext AddImport(Import import)
        {
            Imports.Add(import);
            return this;
        }

        public ScriptContext AddValue(string name, object? value)
            => AddValue(new ExposedValue(name, value));

        public ScriptContext AddValue(ExposedValue value)
        {
            ExposedVariables.Add(value);
            return this;
        }

        public ScriptContext AddDelegate(string name, Delegate method)
            => AddDelegate(new ExposedDelegate(name, method));

        public ScriptContext AddDelegate(ExposedDelegate value)
        {
            ExposedDelegates.Add(value);
            return this;
        }

        public ScriptContext Clone()
        {
            return this;
        }
    }
}
