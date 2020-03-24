using System;
using Wallop.Types.Loading;
using Wallop.Cmd;

namespace Wallop.Controller
{
    class Program
    {
        private static TypeLoader _typeLoader;

        static int Main(string[] args)
        {
            _typeLoader = new TypeLoader();

            var commandSet = GetCommandSet();

            commandSet.Commands.Add(Cmd.Command.Create(name: "repl").Action(Repl));

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
            //New resource.
            var command = Cmd.Command.Create(name: "new", helpText: "Provides the creation of resources.")
                .AddOption(o => o.Set(name: "connection", flag: true, group: "conn"))
                .AddOption(o => o.Set(name: "layout", flag: true, group: "lyt"))
                .AddOption(o => o.Set(name: "layer", flag: true, group: "lyr"))
                .AddOption(o => o.Set(name: "name", required: true, helpText: "The new resource's name."))
                .Action(HandleNewCommand);

            var ipc = _typeLoader.Load<IPC.IPCClient>(Types.Defaults.GetDefaultLibrary(Types.DefaultImplementedLibraries.IPC));
            foreach (var item in ipc.GetOptions())
            {
                command.AddOption(item);
            }

            return command;
        }

        private static void Repl(Cmd.ParseResults results)
        {
            // We can't re-use the commandset from above because then you'd be able to cause an infinite loop of Repl commands.
            var commandSet = GetCommandSet();
            var parser = new Cmd.Parser(commandSet);

            string input = Console.ReadLine();
            while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                var parseResults = parser.Parse(input);
                input = Console.ReadLine();
            }
        }

        private static void HandleNewCommand(Cmd.ParseResults results)
        {
            if (results.Flag("connection"))
            {
                _typeLoader.LoadFromCache<IPC.IPCClient>(out var ipcClient);
                ipcClient.CreateActiveConnection(results["name"], results);
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
