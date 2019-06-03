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

        public void New()
        {
            Layers.Clear();
        }

        public void Load(string file)
        {
            New();
            Layers = JsonConvert.DeserializeObject<List<LayerSettings>>(File.ReadAllText(file));
        }

        public void Save(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(Layers));
        }

        public void EnterPreview()
        {
            _previewBackupFile = Path.GetTempFileName();
            Save(_previewBackupFile);
        }

        public void ExitPreview(bool apply)
        {
            if(!apply)
            {
                Load(_previewBackupFile);
            }
            File.Delete(_previewBackupFile);
        }
    }
}
