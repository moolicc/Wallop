using System;
using Wallop.Types.Loading;

namespace Wallop.Controller
{
    class Program
    {
        private static TypeLoader _typeLoader;

        static void Main(string[] args)
        {
            _typeLoader = new TypeLoader();

            Console.WriteLine("Hello World!");
        }

        private static Cmd.CommandSet GetCommandSet()
        {
            Cmd.CommandSet commandSet = new Cmd.CommandSet();

            commandSet.Commands.Add(GetNewCommand());

            return commandSet;
        }

        private static Cmd.Command GetNewCommand()
        {
            var command = Cmd.Command.Create("new")
                .AddOption(o => o.Set(name: "connection", flag: true, group: "conn"))
                .AddOption(o => o.Set(name: "layout", flag: true, group: "lyt"))
                .AddOption(o => o.Set(name: "layer", flag: true, group: "lyr"));

            var ipc = _typeLoader.LoadFromLibrary<IPC.IPCClient>(DefaultImplementedLibraries.IPC);
            foreach (var item in ipc.GetOptions())
            {
                command.AddOption(item);
            }

            return command;
        }
    }
}
