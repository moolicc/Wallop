using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.ECS.ActorQuerying.Parsing.Tokens.Default
{
    public class FilterToken : IToken
    {
        public string Value { get; private set; }
        public int Index { get; private set; }

        public FilterToken(int index, string keyword = "filter")
        {
            Index = index;
            Value = keyword;
        }
    }
}
