
using Engine;

Console.WriteLine("Hello, World!");

var app = new EngineApp();
app.Setup(Veldrid.GraphicsBackend.Direct3D11);
app.Run();
