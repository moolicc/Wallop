using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class LastToken : IToken
    {
        public string Value { get; init; }

        public int Index { get; init; }

        public LastToken(int index, string keyword = "??")
        {
            Index = index;
            Value = keyword;
        }
    }
}
