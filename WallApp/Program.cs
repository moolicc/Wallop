using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallApp;

namespace WallApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            App app = new App();
            app.InitializeComponent();

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
