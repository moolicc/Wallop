using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Messaging;

namespace Wallop.Types.Plugins.EndPoints
{
    public class EngineStartupEndPoint : EndPointBase
    {
        public List<System.CommandLine.Command> CommandLineVerbs { get; private set; }

        public EngineStartupEndPoint(Messenger messages)
            : base(messages)
        {
            CommandLineVerbs = new List<System.CommandLine.Command>();
        }
    }
}
