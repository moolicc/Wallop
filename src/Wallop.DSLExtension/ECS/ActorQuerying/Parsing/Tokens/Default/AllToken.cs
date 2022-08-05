using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class AllToken : IToken
    {
        public string Value { get; private set; }
        public int Index { get; private set; }

        public AllToken(int index, string keyword = "*")
        {
            Index = index;
            Value = keyword;
        }
    }
}
