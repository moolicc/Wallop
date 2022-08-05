using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class RealToken : IToken
    {
        public string Value { get; init; }

        public int Index { get; init; }
        public double ValueF { get; init; }

        public RealToken(int index, string value)
        {
            Value = value;
            Index = index;
            ValueF = double.Parse(value);
        }
    }
}
