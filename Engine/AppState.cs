using WallApp.Services;

namespace WallApp
{
    [Service]
    class AppState
    {
        public bool RunningIntro { get; private set; }
        public bool RunningOutro { get; private set; }

    }
}
