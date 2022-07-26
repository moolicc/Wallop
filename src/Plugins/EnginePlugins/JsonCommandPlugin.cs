using EnginePlugins.Overlay.Windows;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wallop.Types.Plugins.EndPoints;

namespace EnginePlugins
{
    public class JsonCommandPlugin
    {
        [PluginPantry.Extending.PluginEntryPoint("Json Command", "1.0.0.0")]
        public void PluginEntryPoint(PluginPantry.Extending.PluginInformation pluginInfo)
        {
            pluginInfo.Exposed.RegisterEndPoint<Wallop.Types.Plugins.EndPoints.EngineStartupEndPoint>(HandleCommandLine, pluginInfo.PluginId);
        }

        public void HandleCommandLine(Wallop.Types.Plugins.EndPoints.EngineStartupEndPoint context)
        {
            var sourceArg = new Argument<string>("source", "The source or filepath to the source of the json.");
            var command = new Command("json", "Executes commands contained in a json string or file.")
            {
                sourceArg
            };
            command.SetHandler(s =>
            {
                if(File.Exists(s))
                {
                    s = File.ReadAllText(s);
                }

                
                var messages = Wallop.Messaging.Messages.Json.Json.ParseMessages(s);

                foreach (var item in messages)
                {
                    context.Messages.Put(item.Value, item.MessageType);
                }
            }, sourceArg);

            context.CommandLineVerbs.Add(command);
        }
    }
}
