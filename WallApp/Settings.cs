using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WallApp
{
    class Settings
    {
        public static Settings Instance { get; private set; }
        
        static Settings()
        {
            Instance = new Settings();
        }

        public static void Load(string file)
        {
            string data = File.ReadAllText(file);
            Instance = JsonConvert.DeserializeObject<Settings>(data);
        }

        public static void Save(string file)
        {
            string data = JsonConvert.SerializeObject(Instance);
            File.WriteAllText(file, data);
        }

        public double BackBufferWidthFactor { get; set; }
        public double BackBufferHeightFactor { get; set; }
        public int FrameRate { get; set; }

        private Settings()
        {
            BackBufferWidthFactor = 0.5D;
            BackBufferHeightFactor = 0.5D;
            FrameRate = 60;
        }
    }
}
