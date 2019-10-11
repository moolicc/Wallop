using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WallApp.Bridge
{
    public class Slave : IOPipe
    {
        public Slave()
        {
            var reader = new StreamReader(Console.OpenStandardInput());
            var writer = new StreamWriter(Console.OpenStandardOutput());
            InputStream = reader;
            OutputStream = writer;
        }
    }
}
