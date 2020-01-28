using System;
using System.IO;

namespace Wallop.Bridge
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
