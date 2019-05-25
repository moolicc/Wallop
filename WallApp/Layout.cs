using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WallApp
{
    static class Layout
    {
        public static List<LayerSettings> Layers { get; private set; }

        private static string _previewBackupFile;

        static Layout()
        {
            Layers = new List<LayerSettings>();
        }

        public static void New()
        {
            Layers.Clear();
        }

        public static void Load(string file)
        {
            New();
            Layers = JsonConvert.DeserializeObject<List<LayerSettings>>(File.ReadAllText(file));
        }

        public static void Save(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(Layers));
        }

        public static void EnterPreview()
        {
            _previewBackupFile = Path.GetTempFileName();
            Save(_previewBackupFile);
        }

        public static void ExitPreview(bool apply)
        {
            if(!apply)
            {
                Load(_previewBackupFile);
            }
            File.Delete(_previewBackupFile);
        }
    }
}
