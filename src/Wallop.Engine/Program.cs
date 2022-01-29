
using System.Reflection;
using Wallop.Engine;

Console.WriteLine("Hello, World!");

var typedSource = new Cog.Sources.TypedSettingSource();
var jsonSource = new Cog.Sources.JsonSettingsSource("esettings.json");
var engineConfig = new Cog.Configuration();

engineConfig.Options.Sources.Add(typedSource);
engineConfig.Options.Sources.Add(jsonSource);
engineConfig.Options.ConfigureBindings = false;

engineConfig.LoadSettingsAsync().Wait();

Console.WriteLine("Loaded configuration:");
engineConfig.ResolveBindingsAsync<Wallop.Engine.Settings.GraphicsSettings>().Wait();
foreach (var item in engineConfig.GetValues())
{
    Console.WriteLine("{0}: {1}", item.Key, item.Value);
}


var pluginLoader = new PluginPantry.PluginLoader();
//var plugins = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\Plugins\TestPlugin\bin\Debug\net6.0\TestPlugin.dll");
var morePlugins = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\EnginePlugins\bin\Debug\net6.0\EnginePlugins.dll");
var yetMore = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\HostApis\bin\Debug\net6.0\HostApis.dll");
var moarPlugions = pluginLoader.LoadPluginAssembly(@"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\Scripting.IronPython\bin\Debug\net6.0\Scripting.IronPython.dll");

var context = new PluginPantry.PluginContext();
//context.IncludePlugins(plugins);
context.IncludePlugins(morePlugins);
context.IncludePlugins(yetMore);
context.IncludePlugins(moarPlugions);
context.BeginPluginExecution(new Wallop.Engine.Types.Plugins.EndPoints.EntryPointContext());


AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler((o, e) =>
{
    Console.WriteLine("Unloaded dependency found: <{0}> on behalf of {1}.", e.Name, e.RequestingAssembly?.GetName().Name ?? "[Not specified]");
    
    var searchDirectories = new[]
    {
        @"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\Scripting.IronPython\bin\Debug\net6.0",
        @"C:\Users\joel\source\repos\moolicc\Wallop\src\Plugins\HostApis\bin\Debug\net6.0",
    };
    int subLength = e.Name.Length;
    if(e.Name.Contains(','))
    {
        subLength = e.Name.IndexOf(',');
    }
    var fileName = $"{e.Name.Substring(0, subLength)}.dll";

    Console.WriteLine("Searching for reference assembly {0} to resolve...", fileName, e.Name);

    foreach (var dir in searchDirectories)
    {
        var combined = Path.Combine(dir, fileName);
        if (File.Exists(combined))
        {
            Console.WriteLine("Assembly found at {0}.", combined);
            return Assembly.LoadFile(combined);
        }
    }

    Console.WriteLine("No assembly found.");
    return null;
});

var app = new EngineApp(engineConfig, context);
app.Setup();
