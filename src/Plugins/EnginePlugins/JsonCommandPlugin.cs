using EnginePlugins.Overlay.Windows;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.Messaging.Messages.Json;
using Wallop.Types.Plugins.EndPoints;

namespace EnginePlugins
{

    public class JsonCommandPlugin
    {
        [PluginPantry.Extending.PluginEntryPoint("Json Command", "1.0.0.0")]
        public void PluginEntryPoint(PluginPantry.Extending.PluginInformation pluginInfo)
        {
            pluginInfo.Exposed.RegisterEndPoint<EngineStartupEndPoint>(HandleCommandLine, pluginInfo.PluginId);
        }

        public void HandleCommandLine(Wallop.Types.Plugins.EndPoints.EngineStartupEndPoint context)
        {
            var sourceArg = new Argument<string>("source", description: "The source or filepath to the source of the json. NOTE (if providing raw json): You may escape double quotes around json values with single quotes.");
            var command = new Command("json", "Executes commands contained in a json string or file.")
            {
                sourceArg
            };
            //command.Handler = new MyHandler(context, sourceArg);


            command.SetHandler(s =>
            {

                if (File.Exists(s))
                {
                    s = File.ReadAllText(s);
                }
                else
                {
                    // NOTE: This allows strings provided via raw json within the command line argument to escape quotes around json values with single quotes.
                    s = s.Replace('\'', '"');
                }

                var messages = Json.ParseMessages(s);

                foreach (var item in messages)
                {
                    context.Messages.Put(item.Value, item.MessageType);
                }
            }, sourceArg);

            context.CommandLineVerbs.Add(command);
        }
    }
}
