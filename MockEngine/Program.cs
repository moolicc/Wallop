using System;
using System.Diagnostics;

namespace MockEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var slave = new WallApp.Bridge.Slave();
                var reader = new WallApp.Bridge.InputReader<WallApp.Bridge.Data.IPayload>(slave);
                using(var s = new System.IO.StreamWriter("MockEngine_Output.txt"))
                {
                    s.AutoFlush = true;
                    while (true)
                    {
                        if (reader.Queue.TryDequeue(out var payload))
                        {
                            s.WriteLine("Payload received");
                            s.WriteLine("  " + payload.ToString());
                        }
                        System.Threading.Thread.Sleep(500);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }
    }
}
