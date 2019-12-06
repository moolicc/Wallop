using System.Collections.Generic;

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
