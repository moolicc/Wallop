using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions
{
    public interface IExpression
    {
        void Evaluate(FilterMachine.Machine machine);
    }
}
