using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd.Parsing
{
    public class ParseResults
    {
        internal static ParseResults EmptySource =>
            new ParseResults()
            {
            };

        public bool HasErrors { get; private set; }
        public ParseErrors Errors { get; private set; }
    }
}
