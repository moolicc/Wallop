
using Wallop.Engine;

Console.WriteLine("Hello, World!");

var typedSource = new Cog.Sources.TypedSettingSource();
var jsonSource = new Cog.Sources.JsonSettingsSource("esettings.json");
var config = new Cog.Configuration();

config.Options.Sources.Add(typedSource);
config.Options.Sources.Add(jsonSource);
config.Options.ConfigureBindings = false;

config.LoadSettingsAsync().Wait();

Console.WriteLine("Loaded configuration:");
config.ResolveBindingsAsync<Wallop.Engine.Settings.GraphicsSettings>().Wait();
foreach (var item in config.GetValues())
{
    Console.WriteLine("{0}: {1}", item.Key, item.Value);
}


var pluginLoader = new PluginPantry.PluginLoader();
//var plugins = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\Plugins\TestPlugin\bin\Debug\net6.0\TestPlugin.dll");
var morePlugins = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\EnginePlugins\bin\Debug\net6.0\EnginePlugins.dll");

var context = new PluginPantry.PluginContext();
//context.IncludePlugins(plugins);
context.IncludePlugins(morePlugins);
context.BeginPluginExecution(new Wallop.Engine.Types.Plugins.EndPoints.EntryPointContext());

var app = new EngineApp(config, context);
app.Setup();
app.Run();
