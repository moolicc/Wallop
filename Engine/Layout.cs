using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WallApp
{
    [Services.Service(Key = "/main")]
    class Layout
    {
        public List<LayerSettings> Layers { get; private set; }

        private string _previewBackupFile;

        public Layout()
        {
            Layers = new List<LayerSettings>();
        }
    }
}
