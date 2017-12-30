
#r "PresentationCore"
#r "PresentationFramework"
#r "WindowsFormsIntegration"
#r "System.Drawing"

using YoutubeExplode;

GetController = new Func<Controller>(() =>
{
    return new control();
});


class control : Controller
{    
    public override void Setup()
    {
        var client = new YoutubeClient();

        System.Console.WriteLine("1");

        var streamInfoTask = client.GetVideoMediaStreamInfosAsync("ueupsBPNkSc");
        streamInfoTask.Wait();
        var streamInfo = streamInfoTask.Result;

        System.Console.WriteLine("1");

        var mediaElement = new System.Windows.Controls.MediaElement();
        System.Console.WriteLine("1");
        mediaElement.Source = new Uri(streamInfo.HlsLiveStreamUrl);
        System.Console.WriteLine("1");

        var elementHost = new System.Windows.Forms.Integration.ElementHost();
        System.Console.WriteLine("1");
        elementHost.Size = new System.Drawing.Size(ScaledLayerBounds.Width, ScaledLayerBounds.Height);
        System.Console.WriteLine("1");
        elementHost.Location = new System.Drawing.Point(ScaledLayerBounds.X, ScaledLayerBounds.Y);
        System.Console.WriteLine("1");
        elementHost.Child = mediaElement;
        System.Console.WriteLine("1");

        PlaceControl(elementHost);
        System.Console.WriteLine("1");
        mediaElement.LoadedBehavior = System.Windows.Controls.MediaState.Manual;
        mediaElement.Play();
        System.Console.WriteLine("1");
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