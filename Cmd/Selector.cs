using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.Cmd
{
    public class Selector : Argument
    {
        protected Selector(string helpText, string selectionGroup)
            : base(selectionGroup, '\0', helpText, selectionGroup)
        {
        }
    }
}
