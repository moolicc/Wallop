
GetController = new Func<Controller>(() =>
{
    return new control();
});

GetViewModel = new Func<LayerSettings, object>((s) =>
{
    return new ViewModel(s);
});

class ViewModel : INotifyPropertyChanged
{
    public string ImagePath
    {
        get => _imagePath;
        set
        {
            if (value == _imagePath)
            {
                return;
            }
            _imagePath = value;
            RaisePropertyChange(nameof(ImagePath));
            _layerSettings["imagefile"] = value;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private string _imagePath;
    private LayerSettings _layerSettings;

    public ViewModel(LayerSettings layerSettings)
    {
        _layerSettings = layerSettings;
        ImagePath = _layerSettings["imagefile"];
    }

    private void RaisePropertyChange(string property)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}

class control : Controller
{
    private Rectangle _drawLocation;
    private SpriteBatch _spriteBatch;
    private Texture2D _texture;
    
    public override void Setup()
    {
        try
        {
            _spriteBatch = new SpriteBatch(Rendering.GraphicsDevice);
            using (var fs = new FileStream(Settings["imagefile"], FileMode.Open))
            {
                _texture = Texture2D.FromStream(Rendering.GraphicsDevice, fs);
            }
            _drawLocation = new Microsoft.Xna.Framework.Rectangle(0, 0, Rendering.ActualWidth, Rendering.ActualHeight);
        }
        catch (Exception ex)
        {
            Interop.NotifyException(ex);
        }
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
        //This is to handle the case where we're drawing from preview and the image hasn't been set yet.
        if(_texture == null)
        {
            return;
        }
        Console.WriteLine(_drawLocation);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_texture, _drawLocation, Color.White);
        _spriteBatch.End();
    }
}