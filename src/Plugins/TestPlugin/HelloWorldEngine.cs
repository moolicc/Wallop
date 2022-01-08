using Wallop.Engine.Types.Plugins;
using PluginPantry.Extending;

namespace TestPlugin
{
    public class HelloWorldEngine
    {
        [PluginPantry.Extending.PluginEntryPoint("Hello world plugin", "1.0.0.0")]
        public void EntryPoint(PluginInformation info)
        {
            Console.WriteLine("Hello world! From {0}", info.PluginId);
            info.Exposed.RegisterEndPoint<EngineStartupEndPoint>("EngineStartup", this, info.PluginId);
        }

        public void EngineStartup(Wallop.Engine.Settings.GraphicsSettings settings)
        {
            Console.WriteLine(settings.ToString());
        }
    }
}