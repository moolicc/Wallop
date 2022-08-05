using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.FilterMachine;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class MemberExpression : IExpression
    {
        public bool HasQualifier => Qualifiers.Count > 0;
        public List<string> Qualifiers { get; private set; }
        public string Name { get; private set; }



        public MemberExpression(string name, params string[] qualifiers)
        {
            Name = name;
            Qualifiers = new List<string>(qualifiers);
        }

        public void Evaluate(Machine machine)
        {
            machine.InvokeMember(Name, Qualifiers.ToArray(), 0);
        }
    }
}
