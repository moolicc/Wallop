using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.ECS.ActorQuerying.FilterMachine;

namespace Wallop.ECS.ActorQuerying.Parsing.Expressions.Default
{
    public class InvocationExpression : IExpression
    {

        public IExpression Member { get; init; }
        public List<IExpression> Arguments { get; init; }

        public InvocationExpression(IExpression member)
        {
            Member = member;
            Arguments = new List<IExpression>();
        }

        public void Evaluate(Machine machine)
        {
            if(Member is MemberExpression mExp)
            {
                foreach (var arg in Arguments)
                {
                    arg.Evaluate(machine);
                }
                machine.InvokeMember(mExp.Name, mExp.Qualifiers.ToArray(), Arguments.Count);
            }
            else
            {
                throw new InvalidOperationException("Invalid member identifier.");
            }
        }
    }
}
