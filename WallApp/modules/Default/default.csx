
GetController = new Func<Controller>(() =>
{
    return new control();
});

GetViewModel = new Func<LayerSettings, object>((s) =>
{
    return null;
});


class control : WallApp.Scripting.Controller
{
    public override void Setup()
    {
        System.Console.WriteLine("Setup called");
    }
    
    public override void EnabledChanged()
    {
    }
    
    public override void Dispose()
    {
    }
    
    public override void Update(GameTime gameTime)
    {
    }
    
    public override void Draw(GameTime gameTime)
    {
    }
}