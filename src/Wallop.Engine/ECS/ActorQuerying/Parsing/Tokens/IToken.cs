using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens
{
    public interface IToken
    {
        string Value { get; }
        int Index { get; }
    }
}
