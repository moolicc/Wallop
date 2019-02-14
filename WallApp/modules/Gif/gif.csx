using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using FrameDimension = System.Drawing.Imaging.FrameDimension;
using System.IO;
using System.Collections.Generic;

GetController = new Func<Controller>(() =>
{
    return new control();
});

GetSettingsController = new Func<SettingsController>(() =>
{
    return new SettingsControl();
});

class SettingsControl : SettingsController
{
    private TextBox textBox;

    public override Control GetSettingsControl()
    {
        textBox = new TextBox();
        // 
        // textBox1
        // 
        textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        textBox.Location = new System.Drawing.Point(3, 3);
        textBox.Size = new System.Drawing.Size(197, 20);
        textBox.Text = Settings["imagefile"];

        // 
        // button1
        // 
        var button = new Button
        {
            Location = new System.Drawing.Point(206, 3),
            Size = new System.Drawing.Size(75, 23),
            UseVisualStyleBackColor = true,
            Text = "Browse"
        };
        button.Click += BrowseClicked;

        //
        // label1
        //
        var label = new Label();
        label.Text = "Selected Image";

        // 
        // tableLayoutPanel1
        // 
        var layoutPanel = new TableLayoutPanel();

        layoutPanel.AutoSize = true;
        layoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        layoutPanel.ColumnCount = 2;
        layoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
        layoutPanel.RowCount = 1;

        layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 100F));
        layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

        layoutPanel.Controls.Add(label, 0, 0);
        layoutPanel.Controls.Add(button, 1, 1);
        layoutPanel.Controls.Add(textBox, 0, 1);

        return layoutPanel;
    }

    public override string ApplyClicked()
    {
        if (string.IsNullOrWhiteSpace(textBox.Text) || !System.IO.File.Exists(textBox.Text))
        {
            return "Please select a valid image file.";
        }
        Settings["imagefile"] = textBox.Text;
        return "";
    }

    public override void CancelClicked()
    {

    }

    private void BrowseClicked(object sender, EventArgs e)
    {
        var fileDialog = new OpenFileDialog();
        fileDialog.Title = "Select Image";
        fileDialog.FileName = textBox.Text;
        fileDialog.Filter = "GIF files (*.gif) | *.gif";

        if (fileDialog.ShowDialog() == DialogResult.OK)
        {
            textBox.Text = fileDialog.FileName;
        }
    }
}

class control : Controller
{
    private Microsoft.Xna.Framework.Rectangle _drawLocation;

    private SpriteBatch _spriteBatch;
    private Texture2D _texture;
    private GifFrame[] _frames;
    private int _curFrame;
    private int _curFrameTime;
    
    public override void Setup()
    {
        _spriteBatch = new SpriteBatch(Rendering.GraphicsDevice);
        string file = Settings["imagefile"];
        _frames = ExtractFrames(file);
        _curFrame = 0;
        _curFrameTime = 0;
        _texture = _frames[0].Texture;

        _drawLocation = new Rectangle(0, 0, Rendering.ActualWidth, Rendering.ActualHeight);
    }
    
    public override void EnabledChanged()
    {
    }
    
    public override void Dispose()
    {
    }
    
    public override void Update(GameTime gameTime)
    {
        _curFrameTime += gameTime.ElapsedGameTime.Milliseconds;
        if(_frames[_curFrame].Duration <= _curFrameTime)
        {
            _curFrameTime = 0;
            _texture = _frames[_curFrame].Texture;

            _curFrame++;
            if(_curFrame >= _frames.Length)
            {
                _curFrame = 0;
            }
        }
    }
    
    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();
        _spriteBatch.Draw(_texture, _drawLocation, Color.White);
        _spriteBatch.End();
    }


    private GifFrame[] ExtractFrames(string path)
    {
        List<GifFrame> gifFrames = new List<GifFrame>();

        Image image = Image.FromFile(path);
        int frameCount = image.GetFrameCount(FrameDimension.Time);

        byte[] times = image.GetPropertyItem(0x5100).Value;
        for (int i = 0; i < frameCount; i++)
        {
            int duration = BitConverter.ToInt32(times, 4 * i) * 10;
            gifFrames.Add(new GifFrame(TextureFromImage((Image)image.Clone()), duration));
            image.SelectActiveFrame(FrameDimension.Time, i);
        }
        image.Dispose();

        return gifFrames.ToArray();
    }

    private Texture2D TextureFromImage(Image image)
    {
        Texture2D texture;
        using (MemoryStream stream = new MemoryStream())
        {
            image.Save(stream, ImageFormat.Bmp);
            stream.Position = 0;
            texture = Texture2D.FromStream(Rendering.GraphicsDevice, stream);
        }
        return texture;
    }

    private class GifFrame
    {
        public Texture2D Texture { get; set; }
        public int Duration { get; private set; }

        public GifFrame(Texture2D texture, int duration)
        {
            Texture = texture;
            Duration = duration;
        }
    }
}