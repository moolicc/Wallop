
using Engine;

Console.WriteLine("Hello, World!");

var typedSource = new Cog.Sources.TypedSettingSource();
var jsonSource = new Cog.Sources.JsonSettingsSource("esettings.json");
var config = new Cog.Configuration();

config.Options.Sources.Add(typedSource);
config.Options.Sources.Add(jsonSource);

config.LoadSettingsAsync().Wait();

Console.WriteLine("Loaded configuration:");
foreach (var item in config.GetValues())
{
    Console.WriteLine("{0}: {1}", item.Key, item.Value);
}

var app = new EngineApp(config);
app.Setup();
app.Run();
