using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class Command
    {
        public string Name { get; private set; }
        public List<Argument> Arguments { get; private set; }
    }
}
