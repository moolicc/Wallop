using System;

namespace WallApp.Engine
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
#if DEBUG
                while (!System.Diagnostics.Debugger.IsAttached)
                {

                }
                System.Diagnostics.Debugger.Break();
#endif
                using (Game game = new Game())
                {
                    game.Run();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void LoadLogs()
        {
        }
    }
}
