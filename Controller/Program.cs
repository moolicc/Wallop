using System;
using Wallop.Types.Loading;

namespace Wallop.Controller
{
    class Program
    {
        private static TypeLoader _typeLoader;

        static int Main(string[] args)
        {
            _typeLoader = new TypeLoader();

            var commandSet = GetCommandSet();
            var parser = new Cmd.Parser(commandSet);
            var parseResults = parser.Parse(Environment.CommandLine);

            Console.WriteLine("Hello World!");

            return 0;
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
                .AddOption(o => o.Set(name: "layer", flag: true, group: "lyr"))
                .Action(HandleNewCommand);

            var ipc = _typeLoader.Load<IPC.IPCClient>(Types.Defaults.GetDefaultLibrary(Types.DefaultImplementedLibraries.IPC));
            foreach (var item in ipc.GetOptions())
            {
                command.AddOption(item);
            }

            return command;
        }

        private static void HandleNewCommand(Cmd.ParseResults results)
        {
            if (results.Flag("connection"))
            {

            }
            else if(results.Flag("layout"))
            {

            }
            else if(results.Flag("layer"))
            {

            }
            else
            {

            }
        }
    }
}
