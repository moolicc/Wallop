using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.FilterMachine;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions.Default
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
            machine.PushMemberInvocation(Name, Qualifiers.ToArray(), 0);
        }
    }
}
