using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp.Scripting
{
    public class Interop
    {
        public bool IsInPreview { get; internal set; }


        private int _layerId;

        public Interop(int layerId)
        {
            _layerId = layerId;
        }

        public void NotifyException(Exception ex)
        {
            System.Windows.MessageBox.Show(ex.ToString());
        }
    }
}
