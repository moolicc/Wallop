using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens.Default
{
    internal class PipeToken : IToken
    {

        public string Value { get; init; }

        public int Index { get; init; }

        public PipeToken(int index, string keyword = "|")
        {
            Index = index;
            Value = keyword;
        }
    }
}
