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
                var slave = new Wallop.Bridge.Slave();
                var reader = new Wallop.Bridge.InputReader<Wallop.Bridge.Data.IPayload>(slave);
                using (var s = new System.IO.StreamWriter("MockEngine_Output.txt"))
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
