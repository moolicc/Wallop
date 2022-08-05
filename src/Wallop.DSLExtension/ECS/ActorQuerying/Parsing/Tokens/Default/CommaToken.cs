using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class CommaToken : IToken
    {
        public string Value => ",";
        public int Index { get; init; }


        public CommaToken(int index)
        {
            Index = index;
        }
    }
}
