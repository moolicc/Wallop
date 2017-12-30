using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
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
