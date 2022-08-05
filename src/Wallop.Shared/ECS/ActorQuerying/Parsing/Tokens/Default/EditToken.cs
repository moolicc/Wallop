using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class EditToken : IToken
    {
        public string Value { get; private set; }
        public int Index { get; private set; }

        public EditToken(int index, string keyword = "edit")
        {
            Index = index;
            Value = keyword;
        }
    }
}
