using System;
using System.Collections.Generic;
using System.Text;

namespace Wallop.UIExtensions
{
    public class Panel
    {
        public event EventHandler OKSelected;
        public event EventHandler CancelSelected;

        public List<Field> Fields { get; private set; }

        public Panel()
        {
            Fields = new List<Field>();
        }

        public void OnOK(EventArgs e)
        {
            OKSelected?.Invoke(this, e);
        }

        public void OnCancel(EventArgs e)
        {
            CancelSelected?.Invoke(this, e);
        }
    }
}
