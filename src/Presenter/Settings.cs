using System.IO;

namespace Wallop.Presenter
{
    class Settings
    {
        public static Settings Instance { get; private set; }

        static Settings()
        {
            Instance = new Settings();
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
