using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.Scripting
{
    public record struct CallFrame(string Namespace, string Function, int LineNumber, bool ExceptionState);

    public class EngineState
    {
        public int LastLine;
        public string ReportedStatus;
        public List<CallFrame> CallStack;

        public EngineState()
        {
            LastLine = -1;
            ReportedStatus = "";
            CallStack = new List<CallFrame>();
        }
    }
}
