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
                //Console.WriteLine("Waiting for debugger to attach...");
                while (!System.Diagnostics.Debugger.IsAttached)
                {

                }
                //Console.WriteLine("Debugger attached.");
                //Console.WriteLine("Once the simulation starts, press 'b' to break execution.");
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
