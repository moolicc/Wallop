using System;

namespace WallApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (Game game = new Game())
            {
                game.Run();
            }
        }

        private static void LoadLogs()
        {
        }
    }
}
