using System;

namespace Controller
{

    // The job of the controller is to send commands to the engine over the IPC.
    // The controller does not run indefinitely, and only exists long enough to send commands to the Engine.

        //controller connect [endpoint]
        //controller new layer module layername
        //controller new layout [base layout]
        //controller remove layer layername
        //controller disconnect
        //controller set layerparam {parameters}

        //Commands can be chained via a semicolon ;.

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
