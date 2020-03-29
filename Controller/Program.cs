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

            if(parseResults.IsEmpty || parseResults.Output.CommandNotFound)
            {
                parseResults = parser.Parse("repl");
            }

            return 0;
        }

        private static Cmd.CommandSet GetCommandSet()
        {
            Cmd.CommandSet commandSet = new Cmd.CommandSet();

            commandSet.Commands.Add(new Command().SetName("exit").SetHelpText("Closes the controller."));
            commandSet.Commands.Add(GetNewCommand());

            commandSet.Commands.Add(Command.Create(name: "help", helpText: "Displays useful information... Maybe...")
                .Action((r) =>
                {
                    if(r.Options.TryGetValue("command", out var commandName))
                    {
                        var command = commandSet.Commands.Find(c => string.Equals(commandName, c.Name, StringComparison.OrdinalIgnoreCase));
                        if (command != null)
                        {
                            Console.WriteLine(command.GetHelpText());
                        }
                        else
                        {
                            Console.WriteLine($"Help: Command '{commandName}' not found.");
                        }
                    }
                    else
                    {
                        Console.WriteLine(commandSet.GenerateHelpText());
                        Console.WriteLine("Run 'help [command]' for detailed information on a specific command.");
                    }
                }).AddOption(Option.Create("command", required: false, helpText: "A command to see detailed information on.")));

            return commandSet;
        }

        private static Cmd.Command GetNewCommand()
        {
            //New resource.
            var command = Cmd.Command.Create(name: "new", helpText: "Provides the creation of resources.")
                .AddOption(o => o.Set(name: "connection", groupSelector: true, groupSelection: "connection"))
                .AddOption(o => o.Set(name: "layout", groupSelector: true, groupSelection: "layout"))
                .AddOption(o => o.Set(name: "layer", groupSelector: true, groupSelection: "layer"))
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
            const string PROMPT = "wallop/controller>";

            string GetInput()
            {
                Console.Write(PROMPT);
                return Console.ReadLine();
            }

            // We can't re-use the commandset from above because then you'd be able to cause an infinite loop of Repl commands.
            var commandSet = GetCommandSet();
            var parser = new Cmd.Parser(commandSet);
            var parseResults = ParseResults.Empty;
            do
            {
                parseResults = parser.Parse(GetInput());
                parseResults.Output.MatchFailed(
                    Failed: message => Console.WriteLine($"{PROMPT}{message}"));
            } while (parseResults.Command != "exit");

            Console.WriteLine("Exiting repl...");
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
