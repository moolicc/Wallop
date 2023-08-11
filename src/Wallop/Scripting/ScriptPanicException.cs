using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Scripting;

namespace Wallop.Scripting
{
    [Serializable]
    public class ScriptPanicException : Exception
    {
        private const string MESSAGE = "Script panicked.";

        public ECS.ScriptedElement? Element { get; private set; }

        public string PanicReason { get; private set; }
        [MemberNotNullWhen(true, nameof(AssociatedEngine))]
        public bool GeneratedByScript { get; private set; }

        public IScriptEngine? AssociatedEngine { get; private set; }


        public ScriptPanicException(string reason, ECS.ScriptedElement element, bool scriptGenerated, IScriptEngine? engine)
            : base(MESSAGE)
        {
            PanicReason = reason;
            Element = element;
            GeneratedByScript = scriptGenerated;
            AssociatedEngine = engine;
        }

        public ScriptPanicException(string reason, ECS.ScriptedElement element, bool scriptGenerated, IScriptEngine? engine, Exception inner)
            : base(MESSAGE, inner)
        {
            PanicReason = reason;
            Element = element;
            GeneratedByScript = scriptGenerated;
            AssociatedEngine = engine;
        }

        public override string ToString()
        {
            const char VERT_T = '├';
            const char VERT_END = '└';
            const char HORI = '─';

            if (!GeneratedByScript)
            {
                return base.ToString();
            }

            var builder = new StringBuilder();

            var state = AssociatedEngine.GetState();
            builder.AppendLine($"ScriptEngine report: {state.ReportedStatus}");
            builder.AppendLine($"Line number: {state.LastLine}");
            builder.AppendLine($"Module: {Element?.ModuleDeclaration.ModuleInfo.SourcePath ?? "??"}");


            for (int i = state.CallStack.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    builder.Append(VERT_END);
                }
                else
                {
                    builder.Append(VERT_T);
                }

                var frame = state.CallStack[i];
                builder.Append(HORI).Append(' ');
                builder.Append(frame.Namespace).Append("::");
                builder.Append(frame.Function).Append(" @ line ").AppendLine(frame.LineNumber.ToString());
            }


            return builder.ToString();
        }
    }
}
