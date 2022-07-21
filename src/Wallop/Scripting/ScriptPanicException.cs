using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Scripting
{
    [Serializable]
    public class ScriptPanicException : Exception
    {
        private const string MESSAGE = "Script panicked.";

        public string PanicReason { get; private set; }
        public bool GeneratedByScript { get; private set; }
        public ECS.ScriptedElement? Element { get; private set; }


        public ScriptPanicException(string reason, ECS.ScriptedElement element, bool scriptGenerated)
            : base(MESSAGE)
        {
            PanicReason = reason;
            Element = element;
            GeneratedByScript = scriptGenerated;
        }

        public ScriptPanicException(string reason, ECS.ScriptedElement element, bool scriptGenerated, Exception inner)
            : base(MESSAGE, inner)
        {
            PanicReason = reason;
            Element = element;
            GeneratedByScript = scriptGenerated;
        }
    }
}
