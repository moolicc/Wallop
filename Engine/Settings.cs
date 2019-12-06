using Newtonsoft.Json;
using System.IO;

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

        public float BackBufferWidthFactor { get; set; }
        public float BackBufferHeightFactor { get; set; }
        public int FrameRate { get; set; }

        private Settings()
        {
            BackBufferWidthFactor = 0.5F;
            BackBufferHeightFactor = 0.5F;
            FrameRate = 60;
        }
    }
}
