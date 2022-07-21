using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Types.Plugins.EndPoints
{
    public class EngineStartupEndPoint : EndPointBase
    {
        public List<System.CommandLine.Command> CommandLineVerbs { get; private set; }

        public EngineStartupEndPoint(Messaging.Messenger messages)
            : base(messages)
        {
            CommandLineVerbs = new List<System.CommandLine.Command>();
        }
    }
}
