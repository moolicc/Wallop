using Wallop.Types.Plugins;
using PluginPantry;

namespace TestPlugin
{
    public class HelloWorldEngine
    {
        [EntryPoint("name", "Hello world plugin", "version", "1.0.0.0")]
        public void EntryPoint(PluginContext context, Guid id)
        {
            Console.WriteLine("Hello world! From {0}", id);
            context.RegisterAction<EngineStartupEndPoint, HelloWorldEngine>(id, nameof(EngineStartup), this);
        }

        public void EngineStartup(Wallop.Settings.GraphicsSettings settings)
        {
            Console.WriteLine(settings.ToString());
        }
    }
}